using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomLogic
{
    class CustomLogicLexer
    {
        protected string _source;
        protected List<CustomLogicToken> _tokens = new List<CustomLogicToken>();

        public CustomLogicLexer(string source)
        {
            _source = source;
        }

        public List<CustomLogicToken> GetTokens()
        {
            _tokens.Clear();
            string[] sourceArr = _source.Split('\n');
            for (int i = 0; i < sourceArr.Length; i++)
            {
                string line = sourceArr[i];
                for (int j = 0; j < line.Length; j++)
                {
                    string twoCharToken = line[j].ToString();
                    if (j < line.Length - 1)
                        twoCharToken += line[j + 1];
                    if (char.IsLetter(line[j]))
                    {
                        string boolStr = ScanBool(j, line);
                        if (boolStr != "")
                        {
                            AddToken(CustomLogicTokenType.Primitive, boolStr == "true", i);
                            j += boolStr.Length - 1;
                        }
                        else
                        {
                            string alphaSymbol = ScanAlphaSymbol(j, line);
                            if (alphaSymbol != "")
                            {
                                if (alphaSymbol == "null")
                                    AddToken(CustomLogicTokenType.Primitive, null, i);
                                else
                                    AddToken(CustomLogicTokenType.Symbol, CustomLogicSymbols.Symbols[alphaSymbol], i);
                                j += alphaSymbol.Length - 1;
                            }
                            else
                            {
                                string name = ScanName(j, line);
                                AddToken(CustomLogicTokenType.Name, name, i);
                                j += name.Length - 1;
                            }
                        }
                    }
                    else if (char.IsDigit(line[j]))
                    {
                        string numberStr = ScanNumber(j, line);
                        if (numberStr.Contains("."))
                            AddToken(CustomLogicTokenType.Primitive, float.Parse(numberStr), i);
                        else
                            AddToken(CustomLogicTokenType.Primitive, int.Parse(numberStr), i);
                        j += numberStr.Length - 1;
                    }
                    else if (line[j] == '\"')
                    {
                        string strLiteral = ScanStringLiteral(j, line);
                        AddToken(CustomLogicTokenType.Primitive, strLiteral, i);
                        j += strLiteral.Length + 1;
                    }
                    else if (CustomLogicSymbols.SpecialSymbolNames.Contains(twoCharToken))
                    {
                        AddToken(CustomLogicTokenType.Symbol, CustomLogicSymbols.Symbols[twoCharToken], i);
                        j += 1;
                    }
                    else if (CustomLogicSymbols.SpecialSymbolNames.Contains(line[j].ToString()))
                    {
                        AddToken(CustomLogicTokenType.Symbol, CustomLogicSymbols.Symbols[line[j].ToString()], i);
                    }
                }
            }
            return _tokens;
        }

        private void AddToken(CustomLogicTokenType type, object value, int line)
        {
            _tokens.Add(new CustomLogicToken(type, value, line + 1 ));
        }

        private string ScanAlphaSymbol(int startIndex, string line)
        {
            string currentLexeme = "";
            for (int i = startIndex; i < line.Length; i++)
            {
                if (!char.IsLetter(line[i]))
                    return "";
                currentLexeme += line[i];
                if (CustomLogicSymbols.AlphaSymbolNames.Contains(currentLexeme))
                    return currentLexeme;
            }
            return "";
        }

        private string ScanBool(int startIndex, string line)
        {
            string currentLexeme = "";
            for (int i = startIndex; i < line.Length; i++)
            {
                if (!char.IsLetter(line[i]))
                    return "";
                currentLexeme += line[i];
                if (currentLexeme == "true" || currentLexeme == "false")
                    return currentLexeme;
            }
            return "";
        }

        private string ScanNumber(int startIndex, string line)
        {
            string currentLexeme = "";
            for (int i = startIndex; i < line.Length; i++)
            {
                if (!char.IsDigit(line[i]) && line[i] != '.')
                    return currentLexeme;
                currentLexeme += line[i];
            }
            return currentLexeme;
        }

        private string ScanName(int startIndex, string line)
        {
            string currentLexeme = "";
            for (int i = startIndex; i < line.Length; i++)
            {
                if (!char.IsLetterOrDigit(line[i]))
                    return currentLexeme;
                currentLexeme += line[i];
            }
            return currentLexeme;
        }

        private string ScanStringLiteral(int startIndex, string line)
        {
            string currentLexeme = "";
            for (int i = startIndex + 1; i < line.Length; i++)
            {
                if (line[i] == '\"')
                    return currentLexeme;
                currentLexeme += line[i];
            }
            return currentLexeme;
        }
    }
}
