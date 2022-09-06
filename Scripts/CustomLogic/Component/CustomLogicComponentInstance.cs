using Map;
using UnityEngine;

namespace CustomLogic
{
    class CustomLogicComponentInstance: CustomLogicClassInstance
    {
        public CustomLogicMapObjectBuiltin MapObject;
        private MapScriptComponent _script;
        public CustomLogicComponentInstance(string name, MapObject obj, MapScriptComponent script): base(name)
        {
            ClassName = name;
            MapObject = new CustomLogicMapObjectBuiltin(obj);
            _script = script;
        }

        public void LoadVariables()
        {
            Variables.Add("MapObject", MapObject);
            foreach (string param in _script.Parameters)
            {
                var arr = param.Split(':');
                string name = arr[0];
                string value = arr[1];
                if (Variables.ContainsKey(name))
                {
                    Variables[name] = DeserializeValue(Variables[name], value);
                }
            }
        }

        private object DeserializeValue(object obj, string value)
        {
            if (value == "null")
                return null;
            if (obj is int)
                return int.Parse(value);
            if (obj is float)
                return float.Parse(value);
            if (obj is string)
                return value;
            if (obj is bool)
                return value == "true";
            if (obj is CustomLogicColorBuiltin)
            {
                string[] strArr = value.Split('-');
                return new CustomLogicColorBuiltin(new Color(float.Parse(strArr[0]), float.Parse(strArr[1]), 
                    float.Parse(strArr[2]), float.Parse(strArr[3])));
            }
            if (obj is CustomLogicVector3Builtin)
            {
                string[] strArr = value.Split('-');
                return new CustomLogicVector3Builtin(new Vector3(float.Parse(strArr[0]), float.Parse(strArr[1]), float.Parse(strArr[2])));
            }
            return null;
        }
    }
}
