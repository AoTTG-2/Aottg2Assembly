using System.Collections.Generic;
using UnityEngine;
using Weather;
using UI;
using Utility;
using GameManagers;
using Events;
using Cameras;
using Map;
using Projectiles;

namespace ApplicationManagers
{
    /// <summary>
    /// Manager used by other classes to load and setup scenes with proper game managers, maps, and cameras.
    /// </summary>
    class SceneLoader : MonoBehaviour
    {
        static SceneLoader _instance;
        public static SceneName SceneName = SceneName.Startup;
        public static BaseGameManager CurrentGameManager;
        public static BaseCamera CurrentCamera;

        public static void Init()
        {
            _instance = SingletonFactory.CreateSingleton(_instance);
            LoadScene(SceneName.Startup);
        }

        public static void LoadScene(SceneName sceneName)
        {
            EventManager.InvokePreLoadScene(sceneName);
            SceneName = sceneName;
            if (sceneName == SceneName.InGame)
                PhotonNetwork.LoadLevel(9);
            else
                Application.LoadLevel(9);
        }

        private static void CreateGameManager()
        {
            if (CurrentGameManager != null)
                Destroy(CurrentGameManager);
            if (SceneName == SceneName.MainMenu)
                CurrentGameManager = Util.CreateDontDestroyObj<MainMenuGameManager>();
            else if (SceneName == SceneName.InGame)
                CurrentGameManager = Util.CreateDontDestroyObj<InGameManager>();
            else if (SceneName == SceneName.CharacterEditor)
                CurrentGameManager = Util.CreateDontDestroyObj<CharacterEditorGameManager>();
            else if (SceneName == SceneName.MapEditor)
                CurrentGameManager = Util.CreateDontDestroyObj<MapEditorGameManager>();
        }

        private static void CreateCamera()
        {
            if (CurrentCamera != null)
                Destroy(CurrentCamera);
            if (SceneName == SceneName.InGame)
                CurrentCamera = Util.CreateDontDestroyObj<InGameCamera>();
            else if (SceneName == SceneName.MapEditor)
                CurrentCamera = Util.CreateDontDestroyObj<MapEditorCamera>();
            else if (SceneName == SceneName.CharacterEditor)
                CurrentCamera = Util.CreateDontDestroyObj<CharacterEditorCamera>();
            else if (SceneName == SceneName.Test)
                CurrentCamera = Util.CreateDontDestroyObj<TestCamera>();
            else
            {
                CurrentCamera = Util.CreateDontDestroyObj<StaticCamera>();
                CurrentCamera.Camera.nearClipPlane = 0.3f;
            }
        }

        private void OnLevelWasLoaded(int level)
        {
            if (level != 9)
                return;
            foreach (GameObject obj in FindObjectsOfType(typeof(GameObject)))
            {
                if (obj.GetComponent<DontDestroyOnLoadTag>() == null && obj.name != "PhotonMono")
                    Destroy(obj);
            }
            CreateGameManager();
            CreateCamera();
            EventManager.InvokeLoadScene(SceneName);
        }
    }

    public enum SceneName
    {
        Startup,
        MainMenu,
        InGame,
        MapEditor,
        CharacterEditor,
        Test
    }
}
