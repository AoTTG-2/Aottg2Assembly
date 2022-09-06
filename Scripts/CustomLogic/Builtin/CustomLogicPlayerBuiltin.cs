using Characters;
using GameManagers;
using System.Collections.Generic;
using UnityEngine;

namespace CustomLogic
{
    class CustomLogicPlayerBuiltin : CustomLogicBaseBuiltin
    {
        public PhotonPlayer Player;

        public CustomLogicPlayerBuiltin(PhotonPlayer player) : base("Player")
        {
            Player = player;
        }

        public override object CallMethod(string methodName, List<object> parameters)
        {
            return null;
        }

        public override object GetField(string name)
        {
            if (name == "Character")
            {
                int viewId = Player.GetIntProperty(PlayerProperty.CharacterViewId, 0);
                if (viewId > 0)
                {
                    var photonView = PhotonView.Find(viewId);
                    if (photonView != null)
                    {
                        var character = photonView.GetComponent<BaseCharacter>();
                        return CustomLogicEvaluator.GetCharacterBuiltin(character);
                    }
                }
                return null;
            }
            else if (name == "ID")
                return Player.ID;
            else if (name == "Name")
                return Player.GetStringProperty(PlayerProperty.DisplayName);
            else if (name == "Guild")
                return Player.GetStringProperty(PlayerProperty.DisplayGuild);
            else if (name == "Team")
                return Player.GetStringProperty(PlayerProperty.Team);
            else if (name == "Status")
                return Player.GetStringProperty(PlayerProperty.Status);
            else if (name == "CharacterType")
                return Player.GetStringProperty(PlayerProperty.Character);
            else if (name == "Kills")
                return Player.GetIntProperty(PlayerProperty.Kills);
            else if (name == "Deaths")
                return Player.GetIntProperty(PlayerProperty.Deaths);
            else if (name == "HighestDamage")
                return Player.GetIntProperty(PlayerProperty.HighestDamage);
            else if (name == "TotalDamage")
                return Player.GetIntProperty(PlayerProperty.TotalDamage);
            return base.GetField(name);
        }

        public override void SetField(string name, object value)
        {
            base.SetField(name, value);
        }
    }
}
