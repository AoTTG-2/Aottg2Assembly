using ApplicationManagers;

using Events;
using GameManagers;
using Map;
using Settings;
using SimpleJSONFixed;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Utility;

namespace CustomLogic
{
    class CustomLogicManager: MonoBehaviour
    {
        public static CustomLogicManager _instance;
        public static CustomLogicEvaluator Evaluator;
        public static bool LogicLoaded;
        public static string Logic;
        public static bool Cutscene;
        public static Vector3 CutsceneCameraPosition;
        public static Vector3 CutsceneCameraRotation;

        public static void Init()
        {
            _instance = SingletonFactory.CreateSingleton(_instance);
            CustomLogicSymbols.Init();
            CustomLogicTransfer.Init();
            EventManager.OnLoadScene += OnLoadScene;
            EventManager.OnPreLoadScene += OnPreLoadScene;
        }

        private static void OnPreLoadScene(SceneName sceneName)
        {
            _instance.StopAllCoroutines();
            Evaluator = null;
            LogicLoaded = false;
            Cutscene = false;
        }

        private static void OnLoadScene(SceneName sceneName)
        {
            if (sceneName == SceneName.InGame)
                StartInGame();
            else
                LogicLoaded = true;
        }

        public static void StartInGame()
        {
            if (PhotonNetwork.isMasterClient)
            {
                InGameGeneralSettings settings = SettingsManager.InGameCurrent.General;
                if (BuiltinLevels.IsLogicBuiltin(settings.GameMode.Value))
                {
                    Logic = BuiltinLevels.LoadLogic(settings.GameMode.Value);
                    CustomLogicTransfer.Start();
                    OnLoadCachedLogicRPC();
                }
                else
                {
                    CustomLogicTransfer.LogicTransferReady = true;
                    RPCManager.PhotonView.RPC("LoadBuiltinLogicRPC", PhotonTargets.All, new object[] { settings.GameMode.Value });
                }
            }
        }

        public static void OnLoadBuiltinLogicRPC(string name, PhotonMessageInfo info)
        {
            if (!info.sender.isMasterClient)
                return;
            Logic = BuiltinLevels.LoadLogic(name);
            CustomLogicTransfer.LogicHash = string.Empty;
            LoadLogic();
        }

        public static void OnLoadCachedLogicRPC(PhotonMessageInfo info = null)
        {
            if (info != null && !info.sender.isMasterClient)
                return;
            LoadLogic();
        }

        public static void LoadLogic()
        {
            PhotonNetwork.player.SetCustomProperty(PlayerProperty.CustomLogicHash, CustomLogicTransfer.LogicHash);
            LogicLoaded = true;
        }

        private void OnPhotonPlayerJoined(PhotonPlayer player)
        {
            if (PhotonNetwork.isMasterClient && CustomLogicTransfer.LogicTransferReady)
            {
                CustomLogicTransfer.Transfer(player);
            }
        }

        public static Dictionary<string, BaseSetting> GetModeSettings(string source)
        {
            var lexer = new CustomLogicLexer(source);
            var parser = new CustomLogicParser(lexer.GetTokens());
            var evaluator = new CustomLogicEvaluator(parser.GetStartAst());
            return evaluator.GetModeSettings();
        }

        public static void StartLogic(Dictionary<string, BaseSetting> modeSettings)
        {
            var lexer = new CustomLogicLexer(Logic);
            var parser = new CustomLogicParser(lexer.GetTokens());
            Evaluator = new CustomLogicEvaluator(parser.GetStartAst());
            Evaluator.Start(modeSettings);
        }

        private void FixedUpdate()
        {
            if (Evaluator != null)
            {
                Evaluator.OnTick();
            }
        }
    }
}
