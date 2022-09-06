using UnityEngine;
using System.Collections;
using Utility;
using System.Collections.Generic;
using Events;
using SimpleJSONFixed;
using System;

namespace ApplicationManagers
{
    /// <summary>
    /// Loads managed text data from pastebin.
    /// </summary>
    public class PastebinLoader : MonoBehaviour
    {
        public static JSONNode Leaderboard;
        public static JSONNode Social;
        public static JSONNode About;
        public static PastebinStatus Status = PastebinStatus.Loading;
        static PastebinLoader _instance;

        // consts
        static readonly string LeaderboardURL = "https://pastebin.com/raw/zptDi9T6";
        static readonly string SocialURL = "https://pastebin.com/raw/zptDi9T6";
        static readonly string AboutURL = "https://pastebin.com/raw/zptDi9T6";

        public static void Init()
        {
            _instance = SingletonFactory.CreateSingleton(_instance);
        }

        public static void LoadPastebin()
        {
            _instance.StartCoroutine(_instance.LoadPastebinCoroutine());
        }

        IEnumerator LoadPastebinCoroutine()
        {
            Status = PastebinStatus.Loading;
            string[] urls = new string[] { LeaderboardURL, SocialURL, AboutURL };
            JSONNode[] nodes = new JSONNode[urls.Length];
            for (int i = 0; i < urls.Length; i++)
            {
                using (WWW www = new WWW(LeaderboardURL))
                {
                    yield return www;
                    if (www.error == null)
                    {
                        try
                        {
                            nodes[i] = JSON.Parse(www.text);
                        }
                        catch (Exception e)
                        {
                            DebugConsole.Log("Error parsing pastebin JSON: " + e.Message);
                        }
                    }
                    else
                    {
                        Debug.Log("Failed to load pastebin link: " + www.error);
                    }
                }
            }
            Leaderboard = nodes[0];
            Social = nodes[1];
            About = nodes[2];
            if (Leaderboard != null && Social != null && About != null)
                Status = PastebinStatus.Loaded;
            else
                Status = PastebinStatus.Failed;
        }
    }

    public enum PastebinStatus
    {
        Loading,
        Loaded,
        Failed
    }
}