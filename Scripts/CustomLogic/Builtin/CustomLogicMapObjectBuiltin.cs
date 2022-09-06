using System.Collections.Generic;
using UnityEngine;
using Map;

namespace CustomLogic
{
    class CustomLogicMapObjectBuiltin: CustomLogicBaseBuiltin
    {
        public MapObject Value;
        public CustomLogicMapObjectBuiltin(MapObject obj): base("MapObject")
        {
            Value = obj;
        }

        public override object CallMethod(string methodName, List<object> parameters)
        {
            if (methodName == "AddBuiltinComponent")
            {
                string name = (string)parameters[0];
                if (name == "Daylight")
                {
                    var light = Value.GameObject.AddComponent<Light>();
                    light.type = LightType.Directional;
                    light.color = ((CustomLogicColorBuiltin)parameters[1]).Value;
                    Debug.Log(light.color);
                    light.intensity = (float)parameters[2];
                    light.shadows = LightShadows.Soft;
                    bool weatherControlled = (bool)parameters[3];
                    if (weatherControlled)
                        MapLoader.Daylight.Add(light);
                }
                else if (name == "Tag")
                {
                    var tag = (string)parameters[1];
                    MapLoader.RegisterTag(tag, Value);
                }
            }
            return null;
        }

        public override object GetField(string name)
        {
            if (name == "Position")
                return new CustomLogicVector3Builtin(Value.GameObject.transform.position);
            if (name == "Rotation")
                return new CustomLogicVector3Builtin(Value.GameObject.transform.rotation.eulerAngles);
            if (name == "Scale")
            {
                var localScale = Value.GameObject.transform.localScale;
                var baseScale = Value.BaseScale;
                return new CustomLogicVector3Builtin(new Vector3(localScale.x / baseScale.x, localScale.y / baseScale.y, localScale.z / baseScale.z));
            }
            if (name == "Name")
                return Value.ScriptObject.Name;
            return null;
        }

        public override void SetField(string name, object value)
        {
            if (name == "Position")
                Value.GameObject.transform.position = ((CustomLogicVector3Builtin)value).Value;
            else if (name == "Rotation")
                Value.GameObject.transform.rotation = Quaternion.Euler(((CustomLogicVector3Builtin)value).Value);
            else if (name == "Scale")
            {
                var localScale = ((CustomLogicVector3Builtin)value).Value;
                var baseScale = Value.BaseScale;
                Value.GameObject.transform.localScale = new Vector3(localScale.x * baseScale.x, localScale.y * baseScale.y, localScale.z * baseScale.z);
            }
        }
    }
}
