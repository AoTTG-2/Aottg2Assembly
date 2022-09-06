using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Utility;
using System.IO;
using CustomLogic;
using Events;

namespace ApplicationManagers
{
    /// <summary>
    /// Main testing module. This can be used to define developer tests or to create debug commands for external testers.
    /// </summary>
    class DebugTesting : MonoBehaviour
    {
        static DebugTesting _instance;

        public static void Init()
        {
            _instance = SingletonFactory.CreateSingleton(_instance);
            EventManager.OnLoadScene += OnLoadScene;
        }

        public static void RunTests()
        {
            if (!ApplicationConfig.DevelopmentMode)
                return;
        }

        public static void RunLateTests()
        {
            if (!ApplicationConfig.DevelopmentMode)
                return;
        }

        private void OnLevelWasLoaded(int level)
        {
        }

        private static void OnLoadScene(SceneName sceneName)
        {
            if (sceneName != SceneName.Test)
                return;
        }

        public static void Log(object message)
        {
            Debug.Log(message);
        }

        void Update()
        {
        }

        public static void RunDebugCommand(string command)
        {
            if (!ApplicationConfig.DevelopmentMode)
            {
                Debug.Log("Debug commands are not available in release mode.");
                return;
            }
            string[] args = command.Split(' ');
            switch (args[0])
            {
                default:
                    Debug.Log("Invalid debug command.");
                    break;
            }
        }
    }
}
