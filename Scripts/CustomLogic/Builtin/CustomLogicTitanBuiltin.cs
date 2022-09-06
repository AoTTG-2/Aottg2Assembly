using Characters;
using System.Collections.Generic;
using UnityEngine;

namespace CustomLogic
{
    class CustomLogicTitanBuiltin : CustomLogicCharacterBuiltin
    {
        public BasicTitan Titan;

        public CustomLogicTitanBuiltin(BasicTitan titan) : base(titan, "Titan")
        {
            Titan = titan;
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
