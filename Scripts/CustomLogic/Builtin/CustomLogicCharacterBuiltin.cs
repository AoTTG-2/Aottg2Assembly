using Characters;
using System.Collections.Generic;
using UnityEngine;

namespace CustomLogic
{
    abstract class CustomLogicCharacterBuiltin: CustomLogicBaseBuiltin
    {
        public BaseCharacter Character;
        public CustomLogicCharacterBuiltin(BaseCharacter character, string type = "Character"): base(type)
        {
            Character = character;
        }

        public override object CallMethod(string name, List<object> parameters)
        {
            return null;
        }

        public override object GetField(string name)
        {
            if (name == "Player")
                return new CustomLogicPlayerBuiltin(Character.Cache.PhotonView.owner);
            else if (name == "IsMine")
                return Character.IsMine();
            else if (name == "IsMainCharacter")
                return Character.IsMainCharacter();
            else if (name == "Position")
                return new CustomLogicVector3Builtin(Character.Cache.Transform.position);
            else if (name == "Rotation")
                return new CustomLogicVector3Builtin(Character.Cache.Transform.rotation.eulerAngles);
            return base.GetField(name);
        }

        public override void SetField(string name, object value)
        {
            if (name == "Position")
                Character.Cache.Transform.position = ((CustomLogicVector3Builtin)value).Value;
            else if (name == "Rotation")
                Character.Cache.Transform.rotation = Quaternion.Euler(((CustomLogicVector3Builtin)value).Value);
        }
    }
}
