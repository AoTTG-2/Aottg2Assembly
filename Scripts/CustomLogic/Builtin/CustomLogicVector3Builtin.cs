using System.Collections.Generic;
using UnityEngine;

namespace CustomLogic
{
    class CustomLogicVector3Builtin: CustomLogicStructBuiltin
    {
        public Vector3 Value = Vector3.zero;

        public CustomLogicVector3Builtin(List<object> parameterValues): base("Vector3")
        {
            if (parameterValues.Count == 0)
                return;
            Value = new Vector3((float)parameterValues[0], (float)parameterValues[1], (float)parameterValues[2]);
        }

        public CustomLogicVector3Builtin(Vector3 value): base("Vector3")
        {
            Value = value;
        }

        public override object CallMethod(string methodName, List<object> parameters)
        {
            return null;
        }
      
        public override object GetField(string name)
        {
            if (name == "X")
                return Value.x;
            else if (name == "Y")
                return Value.y;
            else if (name == "Z")
                return Value.z;
            else if (name == "Normalized")
                return new CustomLogicVector3Builtin(Value.normalized);
            else if (name == "Up")
                return new CustomLogicVector3Builtin(Vector3.up);
            else if (name == "Down")
                return new CustomLogicVector3Builtin(Vector3.down);
            else if (name == "Left")
                return new CustomLogicVector3Builtin(Vector3.left);
            else if (name == "Right")
                return new CustomLogicVector3Builtin(Vector3.right);
            else if (name == "Forward")
                return new CustomLogicVector3Builtin(Vector3.forward);
            else if (name == "Back")
                return new CustomLogicVector3Builtin(Vector3.back);
            return null;
        }

        public override void SetField(string name, object value)
        {
            if (name == "X")
                Value.x = (float)value;
            else if (name == "Y")
                Value.y = (float)value;
            else if (name == "Z")
                Value.z = (float)value;
        }

        public override void Copy(CustomLogicStructBuiltin other)
        {
            var v = (CustomLogicVector3Builtin)other;
            Value = v.Value;
        }
    }
}
