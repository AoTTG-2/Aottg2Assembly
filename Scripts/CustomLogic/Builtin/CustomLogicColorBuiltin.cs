using System.Collections.Generic;
using UnityEngine;

namespace CustomLogic
{
    class CustomLogicColorBuiltin : CustomLogicStructBuiltin
    {
        public Color Value = Color.white;

        public CustomLogicColorBuiltin(List<object> parameterValues) : base("Color")
        {
            if (parameterValues.Count == 0)
                return;
            Value = new Color((float)parameterValues[0], (float)parameterValues[1], 
                (float)parameterValues[2], (float)parameterValues[3]);
        }

        public CustomLogicColorBuiltin(Color value) : base("Color")
        {
            Value = value;
        }

        public override object CallMethod(string methodName, List<object> parameters)
        {
            return null;
        }

        public override object GetField(string name)
        {
            if (name == "R")
                return Value.r;
            else if (name == "G")
                return Value.g;
            else if (name == "B")
                return Value.b;
            else if (name == "A")
                return Value.a;
            return null;
        }

        public override void SetField(string name, object value)
        {
            if (name == "R")
                Value.r = (float)value;
            else if (name == "G")
                Value.g = (float)value;
            else if (name == "B")
                Value.b = (float)value;
            else if (name == "A")
                Value.a = (float)value;
        }

        public override void Copy(CustomLogicStructBuiltin other)
        {
            var v = (CustomLogicColorBuiltin)other;
            Value = v.Value;
        }
    }
}
