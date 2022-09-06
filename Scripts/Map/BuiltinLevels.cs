using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using SimpleJSONFixed;
using ApplicationManagers;
using Settings;
using Events;
using System.IO;
using System.Linq;

namespace Map
{
    class BuiltinLevels
    {
        private static JSONNode _info;
        public static string CustomMapFolderPath = Application.dataPath + "/UserData/CustomMap";
        public static string CustomLogicFolderPath = Application.dataPath + "/UserData/CustomLogic";

        public static void Init()
        {
            EventManager.OnFinishInit += OnFinishInit;
            Directory.CreateDirectory(CustomMapFolderPath);
            Directory.CreateDirectory(CustomLogicFolderPath);
        }

        public static void OnFinishInit()
        {
            _info = JSON.Parse(((TextAsset)AssetBundleManager.MainAssetBundle.Load("BuiltinMapInfo")).text);
        }

        public static string LoadMap(string category, string name)
        {
            if (category == "Custom")
            {
                string path = CustomMapFolderPath + "/" + name + ".txt";
                if (File.Exists(path))
                    return File.ReadAllText(path);
            }
            else
            {
                return AssetBundleManager.TryLoadText(name + "Map");
            }
            return string.Empty;
        }

        public static string LoadLogic(string name)
        {
            foreach (JSONNode gameMode in _info["GameModes"])
            {
                if (gameMode["Name"] == name)
                    return AssetBundleManager.TryLoadText(name + "Logic");
            }
            string path = CustomLogicFolderPath + "/" + name + ".txt";
            if (File.Exists(path))
                return File.ReadAllText(path);
            return string.Empty;
        }

        public static bool IsLogicBuiltin(string name)
        {
            foreach (JSONNode gameMode in _info["GameModes"])
            {
                if (gameMode["Name"] == name)
                    return true;
            }
            return false;
        }

        public static string[] GetMapCategories()
        {
            List<string> categories = new List<string>();
            foreach (JSONNode category in _info["MapCategories"])
                categories.Add(category["Name"]);
            categories.Add("Custom");
            return categories.ToArray();
        }

        public static string[] GetMapNames(string category)
        {
            if (category == "Custom")
            {
                string[] files = GetTxtFiles(CustomMapFolderPath);
                for (int i = 0; i < files.Length; i++)
                    files[i] = files[i].Replace(".txt", "");
                return files;
            }
            else
            {
                List<string> mapNames = new List<string>();
                foreach (JSONNode mapCategory in _info["MapCategories"])
                {
                    if (mapCategory["Name"] == category)
                    {
                        foreach (JSONNode map in mapCategory["Maps"])
                            mapNames.Add(map["Name"]);
                    }
                }
                return mapNames.ToArray();
            }
        }

        public static void DeleteCustomMap(string name)
        {
            File.Delete(CustomMapFolderPath + "/" + name + ".txt");
        }

        public static void SaveCustomMap(string name, MapScript script)
        {
            File.WriteAllText(CustomMapFolderPath + "/" + name + ".txt", script.Serialize());
        }

        public static string[] GetGameModes(string category, string mapName)
        {
            List<string> gameModes = new List<string>();
            foreach (JSONNode gameMode in _info["GameModes"])
                gameModes.Add(gameMode["Name"]);
            JSONNode map = GetMap(category, mapName);
            if (map != null)
            {
                if (map.HasKey("IncludedModes"))
                {
                    gameModes.Clear();
                    foreach (JSONNode mode in map["IncludedModes"])
                        gameModes.Add(mode);
                }
                else if (map.HasKey("ExcludedModes"))
                {
                    foreach (JSONNode node in map["ExcludedModes"])
                    {
                        if (gameModes.Contains(node))
                            gameModes.Remove(node);
                    }
                }
            }
            string[] files = GetTxtFiles(CustomLogicFolderPath);
            foreach (string file in files)
                gameModes.Add(file.Replace(".txt", ""));
            return gameModes.ToArray();
        }

        public static void LoadDefaultSettings(string category, string mapName, string gameMode, string settingCategory, Dictionary<string, BaseSetting> container)
        {
            Dictionary<string, JSONNode> defaultSettings = GetDefaultSettings(category, mapName, gameMode, settingCategory);
            foreach (string key in defaultSettings.Keys)
            {
                JSONNode value = defaultSettings[key];
                if (container.ContainsKey(key))
                {
                    BaseSetting setting = (BaseSetting)container[key];
                    if (setting is BoolSetting)
                        ((BoolSetting)setting).Value = value.AsBool;
                    else if (setting is IntSetting)
                        ((IntSetting)setting).Value = value.AsInt;
                    else if (setting is FloatSetting)
                        ((FloatSetting)setting).Value = value.AsFloat;
                    else if (setting is StringSetting)
                        ((StringSetting)setting).Value = value.ToString();
                }
            }
        }

        private static string[] GetTxtFiles(string path)
        {
            if (Directory.Exists(path))
                return Directory.GetFiles(path, "*.txt", SearchOption.TopDirectoryOnly).Select(f => Path.GetFileName(f)).ToArray();
            return new string[0];
        }

        private static Dictionary<string, JSONNode> GetDefaultSettings(string category, string mapName, string gameMode, string settingCategory)
        {
            JSONNode map = GetMap(category, mapName);
            JSONNode mode = GetGameMode(gameMode);
            Dictionary<string, JSONNode> settings = new Dictionary<string, JSONNode>();
            if (mode.HasKey(settingCategory))
                LoadSettings(settings, mode[settingCategory]);
            if (map.HasKey(settingCategory))
                LoadSettings(settings, map[settingCategory]);
            return settings;
        }

        private static JSONNode GetMap(string category, string mapName)
        {
            foreach (JSONNode node in _info["MapCategories"])
            {
                if (node["Name"] == category)
                {
                    foreach (JSONNode map in node["Maps"])
                    {
                        if (map["Name"] == mapName)
                            return node;
                    }
                }
            }
            return null;
        }

        private static JSONNode GetGameMode(string gameMode)
        {
            foreach (JSONNode node in _info["GameModes"])
            {
                if (node["Name"] == gameMode)
                    return node;
            }
            return null;
        }

        private static void LoadSettings(Dictionary<string, JSONNode> current, JSONNode node)
        {
            foreach (string key in node.Keys)
            {
                if (current.ContainsKey(key))
                    current[key] = node[key];
                else
                    current.Add(key, node[key]);
            }
        }
    }
}
