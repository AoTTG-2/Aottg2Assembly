﻿using ApplicationManagers;
using GameManagers;
using System.Collections.Generic;
using UnityEngine;

namespace CustomLogic
{
    class CustomLogicTimeBuiltin: CustomLogicBaseBuiltin
    {
        public CustomLogicTimeBuiltin(): base("Time")
        {
        }

        public override object CallMethod(string name, List<object> parameters)
        {
            return null;
        }

        public override object GetField(string name)
        {
            if (name == "TickTime")
                return Time.fixedDeltaTime;
            else if (name == "GameTime")
                return Time.time - CustomLogicManager.Evaluator.StartTime;
            return null;
        }

        public override void SetField(string name, object value)
        {
        }
    }
}
