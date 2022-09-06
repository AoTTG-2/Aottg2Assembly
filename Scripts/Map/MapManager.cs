using ApplicationManagers;
using Events;
using GameManagers;
using Settings;
using UnityEngine;
using Utility;
using SimpleJSONFixed;
using System.Collections.Generic;
using System.Linq;

namespace Map
{
    class MapManager: Photon.MonoBehaviour
    {
        public static bool MapLoaded;
        public static MapScript MapScript;
        private static MapManager _instance;

        public static void Init()
        {
            _instance = SingletonFactory.CreateSingleton(_instance);
            MapTransfer.Init();
            MapLoader.Init();
            BuiltinLevels.Init();
            BuiltinMapPrefabs.Init();
            BuiltinMapTextures.Init();
            EventManager.OnLoadScene += OnLoadScene;
            EventManager.OnPreLoadScene += OnPreLoadScene;
        }

        public static Vector3 GetRandomTagPosition(string tag, Vector3 defaultPosition)
        {
            GameObject go = GetRandomTag(tag);
            if (go != null)
                return go.transform.position;
            return defaultPosition;
        }

        public static Vector3 GetRandomTagsPosition(List<string> tags, Vector3 defaultPosition)
        {
            foreach (string tag in tags)
            {
                GameObject go = GetRandomTag(tag);
                if (go != null)
                    return go.transform.position;
            }
            return defaultPosition;
        }

        public static GameObject GetRandomTag(string tag)
        {
            if (MapLoader.Tags.ContainsKey(tag))
            {
                if (MapLoader.Tags[tag].Count > 0)
                {
                    return MapLoader.Tags[tag].GetRandomItem().GameObject;
                }
            }
            return null;
        }

        private static void OnPreLoadScene(SceneName sceneName)
        {
            MapLoaded = false;
        }

        private static void OnLoadScene(SceneName sceneName)
        {
            if (sceneName == SceneName.InGame)
                StartInGame();
            else if (sceneName == SceneName.MapEditor)
                StartMapEditor();
            else
                MapLoaded = true;
        }

        private static void StartInGame()
        {
            if (PhotonNetwork.isMasterClient)
            {
                InGameGeneralSettings settings = SettingsManager.InGameCurrent.General;
                if (settings.MapCategory.Value == "Custom")
                {
                    string map = BuiltinLevels.LoadMap("Custom", settings.MapName.Value);
                    MapScript = new MapScript();
                    MapScript.Deserialize(map);
                    MapTransfer.Start();
                    OnLoadCachedMapRPC();
                }
                else
                {
                    MapTransfer.MapTransferReady = true;
                    RPCManager.PhotonView.RPC("LoadBuiltinMapRPC", PhotonTargets.All, new object[] { settings.MapCategory.Value, settings.MapName.Value });
                }
            }
        }

        private static void StartMapEditor()
        {
            var current = SettingsManager.MapEditorSettings.CurrentMap;
            var maps = BuiltinLevels.GetMapNames("Custom").ToList();
            if (current.Value == string.Empty || !maps.Contains(current.Value))
            {
                if (maps.Count > 0)
                    current.Value = maps[0];
                else
                {
                    current.Value = "Untitled";
                    BuiltinLevels.SaveCustomMap(current.Value, new MapScript());
                }
            }
            MapScript = new MapScript();
            MapScript.Deserialize(BuiltinLevels.LoadMap("Custom", current.Value));
            MapLoader.StartLoadObjects(MapScript.Objects.Objects, true);
        }

        public static void OnLoadBuiltinMapRPC(string category, string name, PhotonMessageInfo info)
        {
            if (!info.sender.isMasterClient)
                return;
            string source = BuiltinLevels.LoadMap(category, name);
            MapScript = new MapScript();
            MapScript.Deserialize(source);
            MapTransfer.MapHash = string.Empty;
            LoadMap();
        }

        public static void OnLoadCachedMapRPC(PhotonMessageInfo info = null)
        {
            if (info != null && !info.sender.isMasterClient)
                return;
            LoadMap();
        }

        public static void LoadMap()
        {
            PhotonNetwork.player.SetCustomProperty("CustomMapHash", MapTransfer.MapHash);
            MapLoader.StartLoadObjects(MapScript.Objects.Objects);
        }

        private void OnPhotonPlayerJoined(PhotonPlayer player)
        {
            if (PhotonNetwork.isMasterClient && MapTransfer.MapTransferReady)
            {
                InGameGeneralSettings settings = SettingsManager.InGameCurrent.General;
                if (settings.MapCategory.Value == "Custom")
                    MapTransfer.Transfer(player);
                else
                    RPCManager.PhotonView.RPC("LoadBuiltinMapRPC", player, new object[] { settings.MapCategory.Value, settings.MapName.Value });
            }
        }
    }
}
