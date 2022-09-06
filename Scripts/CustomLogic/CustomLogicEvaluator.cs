using Characters;
using Map;
using Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomLogic
{
    class CustomLogicEvaluator
    {
        public float StartTime;
        protected CustomLogicStartAst _start;
        protected Dictionary<string, CustomLogicClassInstance> _staticClasses = new Dictionary<string, CustomLogicClassInstance>();
        protected List<CustomLogicClassInstance> _callback = new List<CustomLogicClassInstance>();

        public CustomLogicEvaluator(CustomLogicStartAst start)
        {
            _start = start;
            Init();
        }

        public Dictionary<string, BaseSetting> GetModeSettings()
        {
            Dictionary<string, BaseSetting> settings = new Dictionary<string, BaseSetting>();
            CustomLogicClassInstance instance = _staticClasses["Main"];
            foreach (string variableName in instance.Variables.Keys)
            {
                if (!variableName.StartsWith("_"))
                {
                    object value = instance.Variables[variableName];
                    if (value is float)
                        settings.Add(variableName, new FloatSetting((float)value));
                    else if (value is string)
                        settings.Add(variableName, new StringSetting((string)value));
                    else if (value is int)
                        settings.Add(variableName, new IntSetting((int)value));
                    else if (value is bool)
                        settings.Add(variableName, new BoolSetting((bool)value));
                }
            }
            return settings;
        }

        public void Start()
        {
            Start(new Dictionary<string, BaseSetting>());
        }

        public void Start(Dictionary<string, BaseSetting> modeSettings)
        {
            StartTime = Time.time;
            CustomLogicClassInstance main = _staticClasses["Main"];
            foreach (string variableName in modeSettings.Keys)
            {
                BaseSetting setting = modeSettings[variableName];
                if (setting is FloatSetting)
                    main.Variables[variableName] = ((FloatSetting)setting).Value;
                else if (setting is StringSetting)
                    main.Variables[variableName] = ((StringSetting)setting).Value;
                else if (setting is IntSetting)
                    main.Variables[variableName] = ((IntSetting)setting).Value;
                else if (setting is BoolSetting)
                    main.Variables[variableName] = ((BoolSetting)setting).Value;
            }
            foreach (var instance in _staticClasses.Values)
                EvaluateMethod(instance, "Init", new List<object>());
            foreach (var instance in _callback)
                EvaluateMethod(instance, "Init", new List<object>());
            _callback.Add(_staticClasses["Main"]);
            foreach (var instance in _callback)
                EvaluateMethod(instance, "OnGameStart", new List<object>());
            CustomLogicManager._instance.StartCoroutine(OnSecond());
        }

        public void OnTick()
        {
            foreach (var instance in _callback)
                EvaluateMethod(instance, "OnTick", new List<object>());
        }

        public void OnPlayerSpawn(PhotonPlayer player, BaseCharacter character)
        {
            var playerBuiltin = new CustomLogicPlayerBuiltin(player);
            var characterBuiltin = GetCharacterBuiltin(character);
            foreach (var instance in _callback)
                EvaluateMethod(instance, "OnPlayerSpawn", new List<object>() { playerBuiltin, characterBuiltin });
        }

        public void OnCharacterDie(BaseCharacter victim, BaseCharacter killer)
        {
            var victimBuiltin = GetCharacterBuiltin(victim);
            var killerBuiltin = GetCharacterBuiltin(killer);
            foreach (var instance in _callback)
                EvaluateMethod(instance, "OnCharacterDie", new List<object>() { victimBuiltin, killerBuiltin });
        }

        public void OnChatInput(string message)
        {
            foreach (var instance in _callback)
                EvaluateMethod(instance, "OnChatInput", new List<object>() { message });
        }

        public static CustomLogicCharacterBuiltin GetCharacterBuiltin(BaseCharacter character)
        {
            if (character is Human)
                return new CustomLogicHumanBuiltin((Human)character);
            else if (character is BasicTitan)
                return new CustomLogicTitanBuiltin((BasicTitan)character);
            else if (character is BaseShifter)
                return new CustomLogicShifterBuiltin((BaseShifter)character);
            return null;
        }

        private IEnumerator OnSecond()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                foreach (var instance in new List<CustomLogicClassInstance>(_callback))
                    EvaluateMethod(instance, "OnSecond", new List<object>());
            }
        }

        private void Init()
        {
            foreach (string name in new string[] {"Game", "Vector3", "Color", "Convert", "Cutscene", "Time", "Network"})
                CreateStaticClass(name);
            foreach (string className in new List<string>(_start.Classes.Keys))
            {
                if (className == "Main")
                    CreateStaticClass(className);
                else if ((int)_start.Classes[className].Token.Value == (int)CustomLogicSymbol.Extension)
                    CreateStaticClass(className);
            }
            foreach (CustomLogicClassInstance instance in _staticClasses.Values)
            {
                if (!(instance is CustomLogicBaseBuiltin))
                    RunAssignmentsClassInstance(instance);
            }
            foreach (int id in MapLoader.IdToMapObject.Keys)
            {
                MapObject obj = MapLoader.IdToMapObject[id];
                if (obj.ScriptObject is MapScriptSceneObject)
                {
                    List<MapScriptComponent> components = ((MapScriptSceneObject)obj.ScriptObject).Components;
                    foreach (var component in components)
                    {
                        if (_start.Classes.ContainsKey(component.ComponentName))
                        {
                            CustomLogicComponentInstance instance = CreateComponentInstance(component.ComponentName, obj, component);
                        }
                    }
                }
            }
        }

        private void CreateStaticClass(string className)
        {
            if (!_staticClasses.ContainsKey(className))
            {
                CustomLogicClassInstance instance;
                if (className == "Game")
                    instance = new CustomLogicGameBuiltin();
                else if (className == "Convert")
                    instance = new CustomLogicConvertBuiltin();
                else if (className == "Cutscene")
                    instance = new CustomLogicCutsceneBuiltin();
                else
                    instance = CreateClassInstance(className, new List<object>(), false);
                _staticClasses.Add(className, instance);
            }
        }

        public CustomLogicComponentInstance CreateComponentInstance(string className, MapObject obj, MapScriptComponent script)
        {
            var classInstance = new CustomLogicComponentInstance(className, obj, script);
            RunAssignmentsClassInstance(classInstance);
            classInstance.LoadVariables();
            _callback.Add(classInstance);
            return classInstance;
        }

        public CustomLogicClassInstance CreateClassInstance(string className, List<object> parameterValues, bool init = true)
        {
            CustomLogicClassInstance classInstance;
            if (className == "Dict")
                classInstance = new CustomLogicDictBuiltin();
            else if (className == "List")
                classInstance = new CustomLogicListBuiltin();
            else if (className == "Vector3")
                classInstance = new CustomLogicVector3Builtin(parameterValues);
            else if (className == "Color")
                classInstance = new CustomLogicColorBuiltin(parameterValues);
            else
            {
                classInstance = new CustomLogicClassInstance(className);
                if (init)
                {
                    RunAssignmentsClassInstance(classInstance);
                    EvaluateMethod(classInstance, "Init", parameterValues);
                }
            }
            return classInstance;
        }

        private void RunAssignmentsClassInstance(CustomLogicClassInstance classInstance)
        {
            CustomLogicClassDefinitionAst classAst = _start.Classes[classInstance.ClassName];
            foreach (CustomLogicAssignmentExpressionAst assignment in classAst.Assignments)
            {
                string variableName = ((CustomLogicVariableExpressionAst)assignment.Left).Name;
                object value = EvaluateExpression(classInstance, new Dictionary<string, object>(), assignment.Right);
                if (classInstance.Variables.ContainsKey(variableName))
                    classInstance.Variables[variableName] = value;
                else
                    classInstance.Variables.Add(variableName, value);
            }
        }

        private IEnumerator EvaluateBlockCoroutine(CustomLogicClassInstance classInstance, Dictionary<string, object> localVariables, 
            List<CustomLogicBaseAst> statements)
        {
            ConditionalEvalState conditionalState = ConditionalEvalState.None;
            foreach (CustomLogicBaseAst statement in statements)
            {
                if (statement is CustomLogicAssignmentExpressionAst)
                {
                    CustomLogicAssignmentExpressionAst assignment = (CustomLogicAssignmentExpressionAst)statement;
                    object value = EvaluateExpression(classInstance, localVariables, assignment.Right);
                    if (assignment.Left is CustomLogicVariableExpressionAst)
                    {
                        string variableName = ((CustomLogicVariableExpressionAst)assignment.Left).Name;
                        if (localVariables.ContainsKey(variableName))
                            localVariables[variableName] = value;
                        else
                            localVariables.Add(variableName, value);
                    }
                    else if (assignment.Left is CustomLogicFieldExpressionAst)
                    {
                        CustomLogicFieldExpressionAst fieldExpression = (CustomLogicFieldExpressionAst)assignment.Left;
                        CustomLogicClassInstance fieldInstance = (CustomLogicClassInstance)EvaluateExpression(classInstance, localVariables, fieldExpression.Left);
                        if (fieldInstance is CustomLogicBaseBuiltin)
                            ((CustomLogicBaseBuiltin)fieldInstance).SetField(fieldExpression.FieldName, value);
                        else
                        {
                            if (fieldInstance.Variables.ContainsKey(fieldExpression.FieldName))
                                fieldInstance.Variables[fieldExpression.FieldName] = value;
                            else
                                fieldInstance.Variables.Add(fieldExpression.FieldName, value);
                        }
                    }
                }
                else if (statement is CustomLogicReturnExpressionAst)
                {
                    yield break;
                }
                else if (statement is CustomLogicWaitExpressionAst)
                {
                    object value = EvaluateExpression(classInstance, localVariables, ((CustomLogicWaitExpressionAst)statement).WaitTime);
                    yield return new WaitForSeconds((float)value);
                }
                else if (statement is CustomLogicConditionalBlockAst)
                {
                    CustomLogicConditionalBlockAst conditional = (CustomLogicConditionalBlockAst)statement;
                    if ((int)conditional.Token.Value == (int)CustomLogicSymbol.If)
                    {
                        if ((bool)EvaluateExpression(classInstance, localVariables, conditional.Condition))
                        {
                            yield return CustomLogicManager._instance.StartCoroutine(EvaluateBlockCoroutine(classInstance, localVariables, conditional.Statements));
                            conditionalState = ConditionalEvalState.PassedIf;
                        }
                        else
                            conditionalState = ConditionalEvalState.FailedIf;
                    }
                    else if ((int)conditional.Token.Value == (int)CustomLogicSymbol.While)
                    {
                        while ((bool)EvaluateExpression(classInstance, localVariables, conditional.Condition))
                        {
                            yield return CustomLogicManager._instance.StartCoroutine(EvaluateBlockCoroutine(classInstance, localVariables, conditional.Statements));
                        }
                        conditionalState = ConditionalEvalState.None;
                    }
                    else if ((int)conditional.Token.Value == (int)CustomLogicSymbol.Else)
                    {
                        if ((conditionalState == ConditionalEvalState.FailedIf || conditionalState == ConditionalEvalState.FailedElseIf) &&
                            (bool)EvaluateExpression(classInstance, localVariables, conditional.Condition))
                        {
                            yield return CustomLogicManager._instance.StartCoroutine(EvaluateBlockCoroutine(classInstance, localVariables, conditional.Statements));
                        }
                        conditionalState = ConditionalEvalState.None;
                    }
                    else if ((int)conditional.Token.Value == (int)CustomLogicSymbol.ElseIf)
                    {
                        if ((conditionalState == ConditionalEvalState.FailedIf || conditionalState == ConditionalEvalState.FailedElseIf) &&
                            (bool)EvaluateExpression(classInstance, localVariables, conditional.Condition))
                        {
                            yield return CustomLogicManager._instance.StartCoroutine(EvaluateBlockCoroutine(classInstance, localVariables, conditional.Statements));
                            conditionalState = ConditionalEvalState.PassedElseIf;
                        }
                        else
                            conditionalState = ConditionalEvalState.FailedElseIf;
                    }
                }
                else if (statement is CustomLogicForBlockAst)
                {
                    CustomLogicForBlockAst forBlock = (CustomLogicForBlockAst)statement;
                    EvaluateExpression(classInstance, localVariables, forBlock.Initial);
                    while ((bool)EvaluateExpression(classInstance, localVariables, forBlock.Conditional))
                    {
                        yield return CustomLogicManager._instance.StartCoroutine(EvaluateBlockCoroutine(classInstance, localVariables, forBlock.Statements));
                        EvaluateExpression(classInstance, localVariables, forBlock.Assignment);
                    }
                }
                else if (statement is CustomLogicForeachBlockAst)
                {
                    CustomLogicForeachBlockAst foreachBlock = (CustomLogicForeachBlockAst)statement;
                    //foreach (object variable in (CustomLogicListBuiltin)EvaluateExpression(classInstance, localVariables, foreachBlock.Iterable))
                    //{

                    //}
                }
                else if (statement is CustomLogicBaseExpressionAst)
                    EvaluateExpression(classInstance, localVariables, ((CustomLogicBaseExpressionAst)statement));
                if (!(statement is CustomLogicConditionalBlockAst))
                    conditionalState = ConditionalEvalState.None;
            }
        }

        private object EvaluateBlock(CustomLogicClassInstance classInstance, Dictionary<string, object> localVariables, List<CustomLogicBaseAst> statements)
        {
            ConditionalEvalState conditionalState = ConditionalEvalState.None;
            foreach (CustomLogicBaseAst statement in statements)
            {
                if (statement is CustomLogicAssignmentExpressionAst)
                {
                    CustomLogicAssignmentExpressionAst assignment = (CustomLogicAssignmentExpressionAst)statement;
                    object value = EvaluateExpression(classInstance, localVariables, assignment.Right);
                    if (assignment.Left is CustomLogicVariableExpressionAst)
                    {
                        string variableName = ((CustomLogicVariableExpressionAst)assignment.Left).Name;
                        if (localVariables.ContainsKey(variableName))
                            localVariables[variableName] = value;
                        else
                            localVariables.Add(variableName, value);
                    }
                    else if (assignment.Left is CustomLogicFieldExpressionAst)
                    {
                        CustomLogicFieldExpressionAst fieldExpression = (CustomLogicFieldExpressionAst)assignment.Left;
                        CustomLogicClassInstance fieldInstance = (CustomLogicClassInstance)EvaluateExpression(classInstance, localVariables, fieldExpression.Left);
                        if (fieldInstance is CustomLogicBaseBuiltin)
                            ((CustomLogicBaseBuiltin)fieldInstance).SetField(fieldExpression.FieldName, value);
                        else
                        {
                            if (fieldInstance.Variables.ContainsKey(fieldExpression.FieldName))
                                fieldInstance.Variables[fieldExpression.FieldName] = value;
                            else
                                fieldInstance.Variables.Add(fieldExpression.FieldName, value);
                        }
                    }
                }
                else if (statement is CustomLogicReturnExpressionAst)
                {
                    return EvaluateExpression(classInstance, localVariables, ((CustomLogicReturnExpressionAst)statement).ReturnValue);
                }
                else if (statement is CustomLogicConditionalBlockAst)
                {
                    CustomLogicConditionalBlockAst conditional = (CustomLogicConditionalBlockAst)statement;
                    if ((int)conditional.Token.Value == (int)CustomLogicSymbol.If)
                    {
                        if ((bool)EvaluateExpression(classInstance, localVariables, conditional.Condition))
                        {
                            object value = EvaluateBlock(classInstance, localVariables, conditional.Statements);
                            if (value != null)
                                return value;
                            conditionalState = ConditionalEvalState.PassedIf;
                        }
                        else
                            conditionalState = ConditionalEvalState.FailedIf;
                    }
                    else if ((int)conditional.Token.Value == (int)CustomLogicSymbol.While)
                    {
                        while ((bool)EvaluateExpression(classInstance, localVariables, conditional.Condition))
                        {
                            object value = EvaluateBlock(classInstance, localVariables, conditional.Statements);
                            if (value != null)
                                return value;
                        }
                        conditionalState = ConditionalEvalState.None;
                    }
                    else if ((int)conditional.Token.Value == (int)CustomLogicSymbol.Else)
                    {
                        if ((conditionalState == ConditionalEvalState.FailedIf || conditionalState == ConditionalEvalState.FailedElseIf) && 
                            (bool)EvaluateExpression(classInstance, localVariables, conditional.Condition))
                        {
                            object value = EvaluateBlock(classInstance, localVariables, conditional.Statements);
                            if (value != null)
                                return value;
                        }
                        conditionalState = ConditionalEvalState.None;
                    }
                    else if ((int)conditional.Token.Value == (int)CustomLogicSymbol.ElseIf)
                    {
                        if ((conditionalState == ConditionalEvalState.FailedIf || conditionalState == ConditionalEvalState.FailedElseIf) &&
                            (bool)EvaluateExpression(classInstance, localVariables, conditional.Condition))
                        {
                            object value = EvaluateBlock(classInstance, localVariables, conditional.Statements);
                            if (value != null)
                                return value;
                            conditionalState = ConditionalEvalState.PassedElseIf;
                        }
                        else
                            conditionalState = ConditionalEvalState.FailedElseIf;
                    }
                }
                else if (statement is CustomLogicForBlockAst)
                {
                    CustomLogicForBlockAst forBlock = (CustomLogicForBlockAst)statement;
                    EvaluateExpression(classInstance, localVariables, forBlock.Initial);
                    while ((bool)EvaluateExpression(classInstance, localVariables, forBlock.Conditional))
                    {
                        object value = EvaluateBlock(classInstance, localVariables, forBlock.Statements);
                        if (value != null)
                            return value;
                        EvaluateExpression(classInstance, localVariables, forBlock.Assignment);
                    }
                }
                else if (statement is CustomLogicForeachBlockAst)
                {
                    CustomLogicForeachBlockAst foreachBlock = (CustomLogicForeachBlockAst)statement;
                    //foreach (object variable in (CustomLogicListBuiltin)EvaluateExpression(classInstance, localVariables, foreachBlock.Iterable))
                    //{

                    //}
                }
                else if (statement is CustomLogicBaseExpressionAst)
                    EvaluateExpression(classInstance, localVariables, ((CustomLogicBaseExpressionAst)statement));
                if (!(statement is CustomLogicConditionalBlockAst))
                    conditionalState = ConditionalEvalState.None;
            }
            return null;
        }

        public object EvaluateMethod(CustomLogicClassInstance classInstance, string methodName, List<object> parameterValues)
        {
            if (classInstance is CustomLogicBaseBuiltin)
            {
                return ((CustomLogicBaseBuiltin)classInstance).CallMethod(methodName, parameterValues);
            }
            Dictionary<string, object> localVariables = new Dictionary<string, object>();
            if (!_start.Classes[classInstance.ClassName].Methods.ContainsKey(methodName))
                return null;
            CustomLogicMethodDefinitionAst methodAst = _start.Classes[classInstance.ClassName].Methods[methodName];
            for (int i = 0; i < parameterValues.Count; i++)
                localVariables.Add(methodAst.ParameterNames[i], parameterValues[i]);
            if (methodAst.Coroutine)
            {
                return CustomLogicManager._instance.StartCoroutine(EvaluateBlockCoroutine(classInstance, localVariables, methodAst.Statements));
            }
            else
                return EvaluateBlock(classInstance, localVariables, methodAst.Statements);
        }

        private object EvaluateExpression(CustomLogicClassInstance classInstance, Dictionary<string, object> localVariables, CustomLogicBaseExpressionAst expression)
        {
            if (expression.Type == CustomLogicAstType.PrimitiveExpression)
            {
                return ((CustomLogicPrimitiveExpressionAst)expression).Value;
            }
            else if (expression.Type == CustomLogicAstType.VariableExpression)
            {
                string name = ((CustomLogicVariableExpressionAst)expression).Name;
                if (name == "self")
                    return classInstance;
                else if (_staticClasses.ContainsKey(name))
                    return _staticClasses[name];
                else
                    return localVariables[name];
            }
            else if (expression.Type == CustomLogicAstType.ClassInstantiateExpression)
            {
                CustomLogicClassInstantiateExpressionAst instantiate = (CustomLogicClassInstantiateExpressionAst)expression;
                List<object> parameters = new List<object>();
                foreach (CustomLogicBaseAst ast in instantiate.Parameters)
                {
                    parameters.Add(EvaluateExpression(classInstance, localVariables, (CustomLogicBaseExpressionAst)ast));
                }
                return CreateClassInstance(instantiate.Name, parameters, true);
            }
            else if (expression.Type == CustomLogicAstType.FieldExpression)
            {
                CustomLogicFieldExpressionAst fieldExpression = (CustomLogicFieldExpressionAst)expression;
                CustomLogicClassInstance fieldInstance = (CustomLogicClassInstance)EvaluateExpression(classInstance, localVariables, fieldExpression.Left);
                if (fieldInstance is CustomLogicBaseBuiltin)
                    return ((CustomLogicBaseBuiltin)fieldInstance).GetField(fieldExpression.FieldName);
                return fieldInstance.Variables[fieldExpression.FieldName];
            }
            else if (expression.Type == CustomLogicAstType.NotExpression)
            {
                return !(bool)EvaluateExpression(classInstance, localVariables, ((CustomLogicNotExpressionAst)expression).Next);
            }
            else if (expression.Type == CustomLogicAstType.MethodCallExpression)
            {
                CustomLogicMethodCallExpressionAst methodCallExpression = (CustomLogicMethodCallExpressionAst)expression;
                CustomLogicClassInstance methodCallInstance = (CustomLogicClassInstance)EvaluateExpression(classInstance, localVariables, methodCallExpression.Left);
                List<object> parameters = new List<object>();
                foreach (CustomLogicBaseExpressionAst parameterExpression in methodCallExpression.Parameters)
                    parameters.Add(EvaluateExpression(classInstance, localVariables, parameterExpression));
                return EvaluateMethod(methodCallInstance, methodCallExpression.Name, parameters);
            }
            else if (expression.Type == CustomLogicAstType.BinopExpression)
            {
                CustomLogicBinopExpressionAst binopExpression = (CustomLogicBinopExpressionAst)expression;
                object left = EvaluateExpression(classInstance, localVariables, binopExpression.Left);
                object right = EvaluateExpression(classInstance, localVariables, binopExpression.Right);
                CustomLogicSymbol symbol = (CustomLogicSymbol)binopExpression.Token.Value;
                return EvaluateBinopExpression(symbol, left, right);
            }
            return null;
        }

        private object EvaluateBinopExpression(CustomLogicSymbol symbol, object left, object right)
        {
            Debug.Log(symbol.ToString());
            Debug.Log(left.GetType());
            Debug.Log(right.GetType());
            if (symbol == CustomLogicSymbol.Or)
            {
                return (bool)left || (bool)right;
            }
            else if (symbol == CustomLogicSymbol.And)
            {
                return (bool)left && (bool)right;
            }
            else if (symbol == CustomLogicSymbol.Plus)
            {
                if (left is int && right is int)
                    return (int)left + (int)right;
                else if (left is string && right is string)
                    return (string)left + (string)right;
                else
                    return (float)left + (float)right;
            }
            else if (symbol == CustomLogicSymbol.Minus)
            {
                if (left is int && right is int)
                    return (int)left - (int)right;
                else
                    return (float)left - (float)right;
            }
            else if (symbol == CustomLogicSymbol.Times)
            {
                if (left is int && right is int)
                    return (int)left * (int)right;
                else
                    return (float)left * (float)right;
            }
            else if (symbol == CustomLogicSymbol.Divide)
            {
                if (left is int && right is int)
                    return (int)left / (int)right;
                else
                    return (float)left / (float)right;
            }
            else if (symbol == CustomLogicSymbol.Equals)
                return left.Equals(right);
            else if (symbol == CustomLogicSymbol.NotEquals)
                return !left.Equals(right);
            else if (symbol == CustomLogicSymbol.LessThan)
                return UnboxToFloat(left) < UnboxToFloat(right);
            else if (symbol == CustomLogicSymbol.GreaterThan)
                return UnboxToFloat(left) > UnboxToFloat(right);
            else if (symbol == CustomLogicSymbol.LessThanOrEquals)
                return UnboxToFloat(left) <= UnboxToFloat(right);
            else if (symbol == CustomLogicSymbol.GreaterThanOrEquals)
                return UnboxToFloat(left) >= UnboxToFloat(right);
            return null;
        }

        private float UnboxToFloat(object obj)
        {
            if (obj is int)
                return (float)(int)obj;
            return (float)obj;
        }
    }

    public enum ConditionalEvalState
    {
        None,
        PassedIf,
        FailedIf,
        PassedElseIf,
        FailedElseIf
    }
}