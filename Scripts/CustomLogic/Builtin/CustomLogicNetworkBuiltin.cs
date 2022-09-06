using ApplicationManagers;
using GameManagers;
using System.Collections.Generic;
using UnityEngine;

namespace CustomLogic
{
    class CustomLogicNetworkBuiltin: CustomLogicBaseBuiltin
    {
        public CustomLogicNetworkBuiltin(): base("Network")
        {
        }

        public override object CallMethod(string name, List<object> parameters)
        {
            return null;
        }

        public override object GetField(string name)
        {
            if (name == "IsMasterClient")
                return PhotonNetwork.isMasterClient;
            else if (name == "Players")
            {
                CustomLogicListBuiltin list = new CustomLogicListBuiltin();
                foreach (var player in PhotonNetwork.playerList)
                {
                    list.List.Add(new CustomLogicPlayerBuiltin(player));
                }
                return list;
            }
            else if (name == "MasterClient")
                return new CustomLogicPlayerBuiltin(PhotonNetwork.masterClient);
            else if (name == "MyPlayer")
                return new CustomLogicPlayerBuiltin(PhotonNetwork.player);
            return base.GetField(name);
        }

        public override void SetField(string name, object value)
        {
        }
    }
}
