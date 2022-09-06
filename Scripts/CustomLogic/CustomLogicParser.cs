using ApplicationManagers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomLogic
{
    class CustomLogicParser
    {
        protected List<CustomLogicToken> _tokens = new List<CustomLogicToken>();


        public CustomLogicParser(List<CustomLogicToken> tokens)
        {
            _tokens = tokens;
        }

        public CustomLogicStartAst GetStartAst()
        {
            CustomLogicStartAst start = new CustomLogicStartAst();
                ParseAst(0, start);
            return start;
        }

        public object[] ParseExpressionAst(int index, CustomLogicBaseExpressionAst prev, bool paramExpression = false)
        {
            if (index >= _tokens.Count)
                return new object[] { index, null };
            CustomLogicToken currToken = _tokens[index];
            CustomLogicToken nextToken = null;
            if (index < _tokens.Count - 1)
                nextToken = _tokens[index + 1];
            if (!paramExpression && IsSymbolValue(currToken, (int)CustomLogicSymbol.Semicolon))
            {
                return new object[] { index + 1, prev };
            }
            else if (paramExpression && (IsSymbolValue(currToken, (int)CustomLogicSymbol.Comma) || IsSymbolValue(currToken, (int)CustomLogicSymbol.RightParen)))
            {
                return new object[] { index, prev };
            }
            else if (IsSymbolValue(currToken, (int)CustomLogicSymbol.LeftParen))
            {
                object[] next = ParseExpressionAst(index + 1, prev, true);
                return ParseExpressionAst((int)next[0] + 1, (CustomLogicBaseExpressionAst)next[1]);
            }
            else if (currToken.Type == CustomLogicTokenType.Primitive)
            {
                CustomLogicPrimitiveExpressionAst primitiveExpressionAst = new CustomLogicPrimitiveExpressionAst(currToken.Value, currToken.Line);
                return ParseExpressionAst(index + 1, primitiveExpressionAst, paramExpression);
            }
            else if (IsSymbolValue(currToken, (int)CustomLogicSymbol.Not))
            {
                CustomLogicNotExpressionAst notExpressionAst = new CustomLogicNotExpressionAst(nextToken.Line);
                return ParseExpressionAst(index + 1, notExpressionAst, paramExpression);
            }
            else if (IsSymbolIn(currToken, CustomLogicSymbols.BinopSymbols))
            {
                CustomLogicBinopExpressionAst binopExpressionAst = new CustomLogicBinopExpressionAst(currToken, currToken.Line);
                binopExpressionAst.Left = prev;
                object[] next = ParseExpressionAst(index + 1, binopExpressionAst, paramExpression);
                binopExpressionAst.Right = (CustomLogicBaseExpressionAst)next[1];
                return new object[] { next[0], binopExpressionAst };
            }
            else if (IsSymbolValue(currToken, (int)CustomLogicSymbol.SetEquals))
            {
                CustomLogicAssignmentExpressionAst assignmentAst = new CustomLogicAssignmentExpressionAst(prev, currToken.Line);
                object[] next = ParseExpressionAst(index + 1, null);
                assignmentAst.Right = (CustomLogicBaseExpressionAst)next[1];
                return new object[] { next[0], assignmentAst };
            }
            else if (IsSymbolValue(currToken, (int)CustomLogicSymbol.Dot))
            {
                AssertTokenType(nextToken, CustomLogicTokenType.Name);
                CustomLogicToken peekToken = _tokens[index + 2];
                if (IsSymbolValue(peekToken, (int)CustomLogicSymbol.LeftParen))
                {
                    CustomLogicMethodCallExpressionAst methodCallExpressionAst = new CustomLogicMethodCallExpressionAst((string)nextToken.Value, currToken.Line);
                    methodCallExpressionAst.Left = prev;
                    int scanIndex = index + 3;
                    CustomLogicToken scanToken = _tokens[scanIndex];
                    while (!IsSymbolValue(scanToken, (int)CustomLogicSymbol.RightParen))
                    {
                        if (!IsSymbolValue(scanToken, (int)CustomLogicSymbol.Comma))
                        {
                            object[] expression = ParseExpressionAst(scanIndex, null, true);
                            methodCallExpressionAst.Parameters.Add((CustomLogicBaseAst)expression[1]);
                            int nextIndex = (int)expression[0];
                            if (nextIndex > scanIndex)
                            {
                                scanIndex = nextIndex;
                                scanToken = _tokens[scanIndex];
                            }
                            else
                                AssertFalse(scanToken);
                        }
                        else
                        {
                            scanIndex += 1;
                            scanToken = _tokens[scanIndex];
                        }
                    }
                    return ParseExpressionAst(scanIndex + 1, methodCallExpressionAst, paramExpression);
                }
                else
                {
                    CustomLogicFieldExpressionAst fieldExpressionAst = new CustomLogicFieldExpressionAst((string)nextToken.Value, currToken.Line);
                    fieldExpressionAst.Left = prev;
                    return ParseExpressionAst(index + 2, fieldExpressionAst, paramExpression);
                }
            }
            else if (currToken.Type == CustomLogicTokenType.Name)
            {
                if (IsSymbolValue(nextToken, (int)CustomLogicSymbol.LeftParen))
                {
                    CustomLogicClassInstantiateExpressionAst classExpressionAst = new CustomLogicClassInstantiateExpressionAst((string)currToken.Value, currToken.Line);
                    classExpressionAst.Left = prev;
                    int scanIndex = index + 2;
                    CustomLogicToken scanToken = _tokens[scanIndex];
                    while (!IsSymbolValue(scanToken, (int)CustomLogicSymbol.RightParen))
                    {
                        if (!IsSymbolValue(scanToken, (int)CustomLogicSymbol.Comma))
                        {
                            object[] expression = ParseExpressionAst(scanIndex, null, true);
                            classExpressionAst.Parameters.Add((CustomLogicBaseAst)expression[1]);
                            int nextIndex = (int)expression[0];
                            if (nextIndex > scanIndex)
                            {
                                scanIndex = nextIndex;
                                scanToken = _tokens[scanIndex];
                            }
                            else
                                AssertFalse(scanToken);
                        }
                        else
                        {
                            scanIndex += 1;
                            scanToken = _tokens[scanIndex];
                        }
                    }
                    return ParseExpressionAst(scanIndex + 1, classExpressionAst, paramExpression);
                }
                else
                {
                    CustomLogicVariableExpressionAst variableExpressionAst = new CustomLogicVariableExpressionAst((string)currToken.Value, currToken.Line);
                    return ParseExpressionAst(index + 1, variableExpressionAst, paramExpression);
                }
            }
            return new object[] { index + 1, null };
        }

        public int ParseAst(int index, CustomLogicBaseAst prev)
        {
            int lastIndex = index;
            if (index >= _tokens.Count)
                return index;
            CustomLogicToken currToken = _tokens[index];
            CustomLogicToken nextToken = null;
            if (index < _tokens.Count - 1)
                nextToken = _tokens[index + 1];
            if (prev.Type == CustomLogicAstType.Start)
            {
                if (IsSymbolIn(currToken, CustomLogicSymbols.ClassSymbols))
                {
                    CustomLogicClassDefinitionAst classAst = new CustomLogicClassDefinitionAst(currToken, currToken.Line);
                    AssertSymbolValue(_tokens[index + 2], (int)CustomLogicSymbol.LeftCurly);
                    index = ParseAst(index + 3, classAst);
                    ((CustomLogicStartAst)prev).AddClass((string)nextToken.Value, classAst);
                }
                else
                    AssertFalse(currToken);
            }
            else if (prev.Type == CustomLogicAstType.ClassDefinition)
            {
                if (IsSymbolValue(currToken, (int)CustomLogicSymbol.Function) || IsSymbolValue(currToken, (int)CustomLogicSymbol.Coroutine))
                {
                    AssertTokenType(nextToken, CustomLogicTokenType.Name);
                    bool coroutine = IsSymbolValue(currToken, (int)CustomLogicSymbol.Coroutine);
                    CustomLogicMethodDefinitionAst methodAst = new CustomLogicMethodDefinitionAst(currToken.Line, coroutine);
                    int scanIndex = index + 2;
                    CustomLogicToken scanToken = _tokens[scanIndex];
                    AssertSymbolValue(scanToken, (int)CustomLogicSymbol.LeftParen);
                    scanIndex += 1;
                    scanToken = _tokens[scanIndex];
                    while (!(scanToken.Type == CustomLogicTokenType.Symbol && (int)scanToken.Value == (int)CustomLogicSymbol.RightParen))
                    {
                        if (scanToken.Type == CustomLogicTokenType.Name)
                            methodAst.ParameterNames.Add((string)scanToken.Value);
                        else
                            AssertSymbolValue(scanToken, (int)CustomLogicSymbol.Comma);
                        scanIndex += 1;
                        scanToken = _tokens[scanIndex];
                    }
                    AssertSymbolValue(_tokens[scanIndex + 1], (int)CustomLogicSymbol.LeftCurly);
                    index = ParseAst(scanIndex + 2, methodAst);
                    ((CustomLogicClassDefinitionAst)prev).AddMethod((string)nextToken.Value, methodAst);
                }
                else if (currToken.Type == CustomLogicTokenType.Name)
                {
                    AssertSymbolValue(nextToken, (int)CustomLogicSymbol.SetEquals);
                    CustomLogicVariableExpressionAst variableAst = new CustomLogicVariableExpressionAst((string)currToken.Value, currToken.Line);
                    CustomLogicAssignmentExpressionAst assignmentAst = new CustomLogicAssignmentExpressionAst(variableAst, currToken.Line);
                    object[] expression = ParseExpressionAst(index + 2, assignmentAst);
                    assignmentAst.Right = (CustomLogicBaseExpressionAst)expression[1];
                    index = (int)expression[0];
                    ((CustomLogicClassDefinitionAst)prev).Assignments.Add(assignmentAst);
                }
                else if (IsSymbolValue(currToken, (int)CustomLogicSymbol.RightCurly))
                {
                    return index + 1;
                }
                else
                    AssertFalse(currToken);
            }
            else if (prev.Type == CustomLogicAstType.MethodDefinition || prev.Type == CustomLogicAstType.ConditionalExpression || 
                prev.Type == CustomLogicAstType.ForeachExpression || prev.Type == CustomLogicAstType.ForExpression)
            {
                if (IsSymbolValue(currToken, (int)CustomLogicSymbol.Return))
                {
                    object[] expression = ParseExpressionAst(index + 1, null);
                    index = (int)expression[0];
                    CustomLogicReturnExpressionAst returnExpression = new CustomLogicReturnExpressionAst((CustomLogicBaseExpressionAst)expression[1], currToken.Line);
                    ((CustomLogicBlockAst)prev).Statements.Add(returnExpression);
                }
                else if (IsSymbolValue(currToken, (int)CustomLogicSymbol.Wait))
                {
                    object[] expression = ParseExpressionAst(index + 1, null);
                    index = (int)expression[0];
                    CustomLogicWaitExpressionAst returnExpression = new CustomLogicWaitExpressionAst((CustomLogicBaseExpressionAst)expression[1], currToken.Line);
                    ((CustomLogicBlockAst)prev).Statements.Add(returnExpression);
                }
                else if (currToken.Type == CustomLogicTokenType.Name)
                {
                    object[] expression = ParseExpressionAst(index, null);
                    index = (int)expression[0];
                    ((CustomLogicBlockAst)prev).Statements.Add((CustomLogicBaseExpressionAst)expression[1]);
                }
                else if (IsSymbolIn(currToken, CustomLogicSymbols.ConditionalSymbols))
                {
                    AssertSymbolValue(nextToken, (int)CustomLogicSymbol.LeftParen);
                    CustomLogicConditionalBlockAst conditionalAst = new CustomLogicConditionalBlockAst(currToken, currToken.Line);
                    object[] expression = ParseExpressionAst(index + 2, null, true);
                    conditionalAst.Condition = (CustomLogicBaseExpressionAst)expression[1];
                    index = (int)expression[0];
                    AssertSymbolValue(_tokens[index], (int)CustomLogicSymbol.RightParen);
                    AssertSymbolValue(_tokens[index + 1], (int)CustomLogicSymbol.LeftCurly);
                    index = ParseAst(index + 2, conditionalAst);
                    ((CustomLogicBlockAst)prev).Statements.Add(conditionalAst);
                }
                else if (IsSymbolValue(currToken, (int)CustomLogicSymbol.Foreach))
                {
                    AssertSymbolValue(nextToken, (int)CustomLogicSymbol.LeftParen);
                    CustomLogicForeachBlockAst foreachAst = new CustomLogicForeachBlockAst(currToken.Line);
                    int scanIndex = index + 2;
                    AssertTokenType(_tokens[scanIndex], CustomLogicTokenType.Name);
                    CustomLogicVariableExpressionAst variableAst = new CustomLogicVariableExpressionAst((string)_tokens[scanIndex].Value, _tokens[scanIndex].Line);
                    foreachAst.Variable = variableAst;
                    AssertSymbolValue(_tokens[scanIndex + 1], (int)CustomLogicSymbol.In);
                    object[] expression = ParseExpressionAst(scanIndex + 2, null, true);
                    foreachAst.Iterable = (CustomLogicBaseExpressionAst)expression[1];
                    index = (int)expression[0];
                    AssertSymbolValue(_tokens[index], (int)CustomLogicSymbol.RightParen);
                    AssertSymbolValue(_tokens[index + 1], (int)CustomLogicSymbol.LeftCurly);
                    index = ParseAst(index + 2, foreachAst);
                    ((CustomLogicBlockAst)prev).Statements.Add(foreachAst);
                }
                else if (IsSymbolValue(currToken, (int)CustomLogicSymbol.For))
                {
                    AssertSymbolValue(nextToken, (int)CustomLogicSymbol.LeftParen);
                    CustomLogicForBlockAst forAst = new CustomLogicForBlockAst(currToken.Line);
                    object[] expression = ParseExpressionAst(index + 2, null, true);
                    index = (int)expression[0];
                    AssertSymbolValue(_tokens[index], (int)CustomLogicSymbol.Comma);
                    if (!(expression[1] is CustomLogicAssignmentExpressionAst))
                        AssertFalse(_tokens[index]);
                    forAst.Initial = (CustomLogicAssignmentExpressionAst)expression[1];
                    expression = ParseExpressionAst(index + 1, null, true);
                    index = (int)expression[0];
                    AssertSymbolValue(_tokens[index], (int)CustomLogicSymbol.Comma);
                    forAst.Conditional = (CustomLogicBaseExpressionAst)expression[1];
                    expression = ParseExpressionAst(index + 1, null, true);
                    index = (int)expression[0];
                    AssertSymbolValue(_tokens[index], (int)CustomLogicSymbol.RightParen);
                    if (!(expression[1] is CustomLogicAssignmentExpressionAst))
                        AssertFalse(_tokens[index]);
                    forAst.Assignment = (CustomLogicAssignmentExpressionAst)expression[1];
                    AssertSymbolValue(_tokens[index + 1], (int)CustomLogicSymbol.LeftCurly);
                    index = ParseAst(index + 2, forAst);
                    ((CustomLogicBlockAst)prev).Statements.Add(forAst);

                }
                else if (IsSymbolValue(currToken, (int)CustomLogicSymbol.RightCurly))
                {
                    return index + 1;
                }
                else
                    AssertFalse(currToken);
            }
            if (index == lastIndex)
            {
                AssertFalse(currToken);
            }
            return ParseAst(index, prev);
        }

        private bool IsSymbolIn(CustomLogicToken token, HashSet<int> symbols)
        {
            return token != null && token.Type == CustomLogicTokenType.Symbol && symbols.Contains((int)token.Value);
        }

        private bool IsSymbolValue(CustomLogicToken token, int symbolValue)
        {
            return token != null && token.Type == CustomLogicTokenType.Symbol && (int)token.Value == symbolValue;
        }

        private void AssertSymbolValue(CustomLogicToken token, int symbolValue)
        {
            if (token == null || token.Type != CustomLogicTokenType.Symbol || (int)token.Value != symbolValue)
                throw new Exception("Parsing error at line " + token.Line.ToString() + ", got " + token.Value.ToString()
                    + ", expected " + ((CustomLogicSymbol)symbolValue).ToString());
        }

        private void AssertTokenType(CustomLogicToken token, CustomLogicTokenType type)
        {
            if (token == null || token.Type != type)
                throw new Exception("Parsing error at line " + token.Line.ToString() + ", got " + token.Value.ToString()
                    + ", expected " + type.ToString());
        }

        private void AssertFalse(CustomLogicToken token)
        {
            throw new Exception("Parsing error at line " + token.Line.ToString() + ", got " + token.Value.ToString());
        }

    }
}