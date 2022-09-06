using ApplicationManagers;
using GameManagers;
using System.Collections.Generic;
using UnityEngine;

namespace CustomLogic
{
    class CustomLogicGameBuiltin: CustomLogicBaseBuiltin
    {
        private string _lastSetTopLabel = string.Empty;

        public CustomLogicGameBuiltin(): base("Game")
        {
        }

        public override object CallMethod(string name, List<object> parameters)
        {
            var gameManager = (InGameManager)SceneLoader.CurrentGameManager;
            if (name == "Print")
            {
                string message = (string)parameters[0];
                bool sync = (bool)parameters[1];
            }
            else if (name == "Debug")
            {
                DebugConsole.Log((string)parameters[0], true);
            }
            else if (name == "SetTopLabel")
            {
                string message = (string)parameters[0];
                bool sync = (bool)parameters[1];
                if (sync && PhotonNetwork.isMasterClient)
                {
                    if (message != _lastSetTopLabel)
                        RPCManager.PhotonView.RPC("SetTopLabelRPC", PhotonTargets.All, new object[] { message });
                    _lastSetTopLabel = message;
                }
                else
                    InGameManager.SetTopLabel(message);
            }
            else if (name == "End")
            {
                if (PhotonNetwork.isMasterClient)
                    gameManager.EndGame((string)parameters[0], (float)parameters[1]);
            }
            else if (name == "SpawnTitans")
            {
                if (PhotonNetwork.isMasterClient)
                {
                    for (int i = 0; i < (int)parameters[0]; i++)
                        gameManager.SpawnAITitan();
                }
            }
            else if (name == "SpawnShifters")
            {
                if (PhotonNetwork.isMasterClient)
                {
                    for (int i = 0; i < (int)parameters[0]; i++)
                        gameManager.SpawnAIShifter();
                }
            }
            return base.CallMethod(name, parameters);
        }

        public override object GetField(string name)
        {
            var gameManager = (InGameManager)SceneLoader.CurrentGameManager;
            if (name == "IsEnding")
                return gameManager.IsEnding;
            else if (name == "Titans")
            {
                var list = new CustomLogicListBuiltin();
                foreach (var titan in gameManager.Titans)
                    list.List.Add(new CustomLogicTitanBuiltin(titan));
                return list;
            }
            else if (name == "Shifters")
            {
                var list = new CustomLogicListBuiltin();
                foreach (var shifter in gameManager.Shifters)
                    list.List.Add(new CustomLogicShifterBuiltin(shifter));
                return list;
            }
            else if (name == "Humans")
            {
                var list = new CustomLogicListBuiltin();
                foreach (var human in gameManager.Humans)
                    list.List.Add(new CustomLogicHumanBuiltin(human));
                return list;
            }
            return base.GetField(name);
        }

        public override void SetField(string name, object value)
        {
        }
    }
}
