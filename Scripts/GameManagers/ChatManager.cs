using System.Collections.Generic;
using UnityEngine;
using Weather;
using UI;
using Utility;
using CustomSkins;
using ApplicationManagers;
using System.Diagnostics;

namespace GameManagers
{
    class ChatManager : MonoBehaviour
    {
        private static ChatManager _instance;
        private static List<string> _lines = new List<string>();
        private static readonly int MaxLines = 10;
        public static Dictionary<TextColor, string> ColorTags = new Dictionary<TextColor, string>();

        public static void Init()
        {
            _instance = SingletonFactory.CreateSingleton(_instance);
        }

        public static void LoadTheme()
        {
            ColorTags.Clear();
            foreach (TextColor color in Util.EnumToList<TextColor>())
            {

                Color c = UIManager.GetThemeColor("ChatPanel", "TextColor", color.ToString());
                ColorTags.Add(color, string.Format("{0:X2}{1:X2}{2:X2}", (int)(c.r * 255), (int)(c.g * 255), (int)(c.b * 255)));
            }
        }

        public static void Clear()
        {
            _lines.Clear();
        }

        public static void AddLine(string line)
        {
            _lines.Add(line);
        }


        public static void HandleInput(string input)
        {
            if (input.StartsWith("/"))
                HandleCommand(input.Substring(1).Split(' '));
        }

       
        private static void HandleCommand(string[] args)
        {
            if (args[0] == "restart")
            {

            }
        }
    }

    enum TextColor
    {
        System,
        ScoreboardPlayerID,
        ScoreboardPlayerName
    }
}
