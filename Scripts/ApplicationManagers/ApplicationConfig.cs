using Photon;
using System;
using UnityEngine;
using System.IO;

namespace ApplicationManagers
{
    /// <summary>
    /// Config file to store project-level settings.
    /// </summary>
    class ApplicationConfig
    {
        private static readonly string DevelopmentConfigPath = Application.dataPath + "/DevelopmentConfig";
        public static bool DevelopmentMode = false;

        // kill-switch in case launcher becomes outdated: LauncherVersion.txt on server will be incremented
        // must be a float or else clients will not recognize it
        public const string LauncherVersion = "1.0";

        // must be updated manually every time asset-bundle is changed. Format is YYYYMMDD.
        public const int AssetBundleVersion = 20211122;

        // must be manually changed every update
        public const string GameVersion = "11/22/2021";

        // must be manually updated every compatibility-breaking update
        public const string LobbyVersion = "1042015";

        public static void Init()
        {
            if (File.Exists(DevelopmentConfigPath))
            {
                DevelopmentMode = true;
            }
        }
    }
}
