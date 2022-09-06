using UnityEngine;
using Utility;
using Settings;
using UI;
using Weather;
using System.Collections;
using GameProgress;
using Map;
using GameManagers;
using Events;
using Characters;
using CustomLogic;
using CustomSkins;
using Anticheat;
using System.Diagnostics;

namespace ApplicationManagers
{
    /// <summary>
    /// Application entry point. Runs on main scene load, and handles loading every other manager.
    /// </summary>
    class ApplicationStart : MonoBehaviour
    {
        private static bool _firstLaunch = true;
        private static ApplicationStart _instance;

        public static void Init()
        {
            if (_firstLaunch)
            {
                _firstLaunch = false;
                _instance = SingletonFactory.CreateSingleton(_instance);
                Start();
            }
        }

        private static void Start()
        {
            DebugConsole.Init();
            ApplicationConfig.Init();
            AnticheatManager.Init();
            PhysicsLayer.Init();
            MaterialCache.Init();
            EventManager.Init();
            AutoUpdater.Init();
            SettingsManager.Init();
            FullscreenHandler.Init();
            UIManager.Init();
            AssetBundleManager.Init();
            SnapshotManager.Init();
            CursorManager.Init();
            WeatherManager.Init();
            GameProgressManager.Init();
            SceneLoader.Init();
            MapManager.Init();
            CustomLogicManager.Init();
            ChatManager.Init();
            PastebinLoader.Init();
            MusicManager.Init();
            SoundManager.Init();
            if (ApplicationConfig.DevelopmentMode)
            {
                DebugTesting.Init();
                DebugTesting.RunTests();
            }
            _instance.StartCoroutine(_instance.Load());
        }

        private IEnumerator Load()
        {
            AutoUpdater.StartUpdate();
            while (AutoUpdater.Status == AutoUpdateStatus.Updating)
                yield return null;
            AssetBundleManager.LoadAssetBundle();
            PastebinLoader.LoadPastebin();
            while (AssetBundleManager.Status == AssetBundleStatus.Loading || PastebinLoader.Status == PastebinStatus.Loading)
                yield return null;
            EventManager.InvokeFinishInit();
            HumanSetup.Init();
            SceneLoader.LoadScene(SceneName.MainMenu);
            if (ApplicationConfig.DevelopmentMode)
                DebugTesting.RunLateTests();
        }

        private void OnGUI()
        {
            if (SceneLoader.SceneName == SceneName.Startup || SceneLoader.SceneName == SceneName.MainMenu)
            {
                Texture2D textureBackgroundBlack = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                textureBackgroundBlack.SetPixel(0, 0, new Color(0f, 0f, 0f, 1f));
                textureBackgroundBlack.Apply();
                Texture2D textureBackgroundBlue = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                textureBackgroundBlue.SetPixel(0, 0, new Color(0.08f, 0.3f, 0.4f, 1f));
                textureBackgroundBlue.Apply();
                Texture2D textureBackgroundDarkBlue = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                textureBackgroundDarkBlue.SetPixel(0, 0, new Color(0.125f, 0.164f, 0.266f, 1f));
                textureBackgroundDarkBlue.Apply();
                LegacyPopupTemplate popup = new LegacyPopupTemplate(textureBackgroundDarkBlue, textureBackgroundBlue, new Color(1f, 1f, 1f, 1f),
                        Screen.width / 2f, Screen.height / 2f, 230f, 140f, 2f);
                DrawBackgroundIfLoading(textureBackgroundBlack);
                if (AutoUpdater.Status == AutoUpdateStatus.Updating)
                {
                    popup.DrawPopup("Auto-updating mod...", 130f, 22f);
                }
                else if (AutoUpdater.Status == AutoUpdateStatus.NeedRestart && !AutoUpdater.CloseFailureBox)
                {
                    bool[] buttons = (popup.DrawPopupWithTwoButtons("Mod has been updated and requires a restart.", 190f, 44f, "Restart Now", 90f, "Ignore", 90f, 25f));
                    if (buttons[0])
                    {
                        if (Application.platform == RuntimePlatform.WindowsPlayer)
                            System.Diagnostics.Process.Start(Application.dataPath.Replace("_Data", ".exe"));
                        else if (Application.platform == RuntimePlatform.OSXPlayer)
                            System.Diagnostics.Process.Start(Application.dataPath + "/MacOS/MacTest");
                        Application.Quit();
                    }
                    else if (buttons[1])
                        AutoUpdater.CloseFailureBox = true;
                }
                else if (AutoUpdater.Status == AutoUpdateStatus.LauncherOutdated && !AutoUpdater.CloseFailureBox)
                {

                    if (popup.DrawPopupWithButton("Game launcher is outdated, visit aotrc.weebly.com for a new game version.", 190f, 66f, "Continue", 80f, 25f))
                        AutoUpdater.CloseFailureBox = true;
                }
                else if (AutoUpdater.Status == AutoUpdateStatus.FailedUpdate && !AutoUpdater.CloseFailureBox)
                {
                    if (popup.DrawPopupWithButton("Auto-update failed, check internet connection or aotrc.weebly.com for a new game version.", 190f, 66f, "Continue", 80f, 25f))
                        AutoUpdater.CloseFailureBox = true;
                }
                else if (AutoUpdater.Status == AutoUpdateStatus.MacTranslocated && !AutoUpdater.CloseFailureBox)
                {
                    if (popup.DrawPopupWithButton("Your game is not in the Applications folder, cannot auto-update and some bugs may occur.", 190f, 66f, "Continue", 80f, 25f))
                        AutoUpdater.CloseFailureBox = true;
                }
                else if (AssetBundleManager.Status == AssetBundleStatus.Loading)
                {
                    popup.DrawPopup("Downloading asset bundle...", 170f, 22f);
                }
                else if (AssetBundleManager.Status == AssetBundleStatus.Failed && !AssetBundleManager.CloseFailureBox)
                {
                    if (popup.DrawPopupWithButton("Failed to load asset bundle, check your internet connection.", 190f, 44f, "Continue", 80f, 25f))
                        AssetBundleManager.CloseFailureBox = true;
                }
            }
        }

        void DrawBackgroundIfLoading(Texture2D texture)
        {
            if (AssetBundleManager.Status == AssetBundleStatus.Loading || AutoUpdater.Status == AutoUpdateStatus.Updating)
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), texture);
        }
    }
}