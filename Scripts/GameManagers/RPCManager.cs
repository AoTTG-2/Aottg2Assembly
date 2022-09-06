using System.Collections.Generic;
using UnityEngine;
using Weather;
using UI;
using Map;
using Effects;
using CustomLogic;

namespace GameManagers
{
    class RPCManager: Photon.MonoBehaviour
    {
        public static PhotonView PhotonView;

        [RPC]
        public void TransferLogicRPC(byte[][] strArray, int msgNumber, int msgTotal, PhotonMessageInfo info)
        {
            CustomLogicTransfer.OnTransferLogicRPC(strArray, msgNumber, msgTotal, info);
        }

        [RPC]
        public void LoadBuiltinLogicRPC(string name, PhotonMessageInfo info)
        {
            CustomLogicManager.OnLoadBuiltinLogicRPC(name, info);
        }

        [RPC]
        public void LoadCachedLogicRPC(PhotonMessageInfo info = null)
        {
            CustomLogicManager.OnLoadCachedLogicRPC(info);
        }

        [RPC]
        public void TransferMapRPC(byte[][] strArray, int msgNumber, int msgTotal, PhotonMessageInfo info)
        {
            MapTransfer.OnTransferMapRPC(strArray, msgNumber, msgTotal, info);
        }

        [RPC]
        public void LoadBuiltinMapRPC(string category, string name, PhotonMessageInfo info)
        {
            MapManager.OnLoadBuiltinMapRPC(category, name, info);
        }

        [RPC]
        public void LoadCachedMapRPC(PhotonMessageInfo info = null)
        {
            MapManager.OnLoadCachedMapRPC(info);
        }
        
        [RPC]
        public void RestartGameRPC(PhotonMessageInfo info)
        {
            InGameManager.OnRestartGameRPC(info);
        }

        [RPC]
        public void PlayerInfoRPC(byte[] data, PhotonMessageInfo info)
        {
            InGameManager.OnPlayerInfoRPC(data, info);
        }

        [RPC]
        public void GameSettingsRPC(byte[] data, PhotonMessageInfo info)
        {
            InGameManager.OnGameSettingsRPC(data, info);
        }

        [RPC]
        public void SetWeatherRPC(byte[] currentWeatherJson, byte[] startWeatherJson, byte[] targetWeatherJson, Dictionary<int, float> targetWeatherStartTimes,
            Dictionary<int, float> targetWeatherEndTimes, float currentTime, PhotonMessageInfo info)
        {
            WeatherManager.OnSetWeatherRPC(currentWeatherJson, startWeatherJson, targetWeatherJson, targetWeatherStartTimes, targetWeatherEndTimes, currentTime, info);
        }

        [RPC]
        public void EmoteEmojiRPC(int viewId, string emoji, PhotonMessageInfo info)
        {
            EmoteHandler.OnEmoteEmojiRPC(viewId, emoji, info);
        }

        [RPC]
        public void EmoteTextRPC(int viewId, string text, PhotonMessageInfo info)
        {
            EmoteHandler.OnEmoteTextRPC(viewId, text, info);
        }

        [RPC]
        public void SpawnEffectRPC(string name, Vector3 position, Quaternion rotation, object[] settings, PhotonMessageInfo info)
        {
            EffectSpawner.OnSpawnEffectRPC(name, position, rotation, settings, info);
        }

        [RPC]
        public void SetTopLabelRPC(string message, PhotonMessageInfo info)
        {
            InGameManager.OnSetTopLabelRPC(message, info);
        }


        [RPC]
        public void TestRPC(Color c)
        {
            Debug.Log(c);
        }

        void Awake()
        {
            PhotonView = GetComponent<PhotonView>();
        }
    }
}
