using System.Collections.Generic;
using UnityEngine;

namespace CustomLogic
{
    abstract class CustomLogicBaseInternal: MonoBehaviour
    {
        public CustomLogicBaseInternal(string name, int instanceId)
        {
        }

        public abstract object CallMethod(string name, List<object> parameters);
    }
}
