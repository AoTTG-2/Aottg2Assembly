using System.Collections.Generic;
using UnityEngine;

namespace CustomLogic
{
    class CustomLogicRigidbodyInternal: CustomLogicBaseInternal
    {
        protected Rigidbody _rigidbody;

        public CustomLogicRigidbodyInternal(string name, int instanceId): base(name, instanceId)
        {
            _rigidbody = gameObject.AddComponent<Rigidbody>();
        }

        public override object CallMethod(string methodName, List<object> parameters)
        {
            if (methodName == "SetVelocity")
            {
            }
            return null;
        }
    }
}
