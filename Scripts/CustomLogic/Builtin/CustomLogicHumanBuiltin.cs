using Characters;
using System.Collections.Generic;
using UnityEngine;

namespace CustomLogic
{
    class CustomLogicHumanBuiltin : CustomLogicCharacterBuiltin
    {
        public Human Human;

        public CustomLogicHumanBuiltin(Human human) : base(human, "Human")
        {
            Human = human;
        }

        public override object CallMethod(string methodName, List<object> parameters)
        {
            return null;
        }

        public override object GetField(string name)
        {
            return base.GetField(name);
        }

        public override void SetField(string name, object value)
        {
            base.SetField(name, value);
        }

    }
}
