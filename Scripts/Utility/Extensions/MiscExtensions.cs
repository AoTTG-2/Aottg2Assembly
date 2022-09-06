using GameManagers;
using Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

static class MiscExtensions
{
    static readonly string HexPattern = @"(\[)[\w]{6}(\])";
    static readonly Regex HexRegex = new Regex(HexPattern);

    public static bool IsNullOrEmpty(this string value)
    {
        return value == null || value.Length == 0;
    }

    public static bool GetActive(this GameObject target)
    {
        return target.activeInHierarchy;
    }

    public static string UpperFirstLetter(this string text)
    {
        if (text == string.Empty)
            return text;
        if (text.Length > 1)
            return char.ToUpper(text[0]) + text.Substring(1);
        return text.ToUpper();
    }

    public static string StripHex(this string text)
    {
        return HexRegex.Replace(text, "");
    }

    public static string HexColor(this string text)
    {
        if (text.Contains("]"))
        {
            text = text.Replace("]", ">");
        }
        bool flag2 = false;
        while (text.Contains("[") && !flag2)
        {
            int index = text.IndexOf("[");
            if (text.Length >= (index + 7))
            {
                string str = text.Substring(index + 1, 6);
                text = text.Remove(index, 7).Insert(index, "<color=#" + str);
                int length = text.Length;
                if (text.Contains("["))
                {
                    length = text.IndexOf("[");
                }
                text = text.Insert(length, "</color>");
            }
            else
            {
                flag2 = true;
            }
        }
        if (flag2)
        {
            return string.Empty;
        }
        return text;
    }

    public static string FormatColor(this string text, TextColor color)
    {
        return "<color=#" + ChatManager.ColorTags[color] + ">" + text + "</color>";
    }

    public static T ToEnum<T>(this string value, bool ignoreCase = true)
    {
        if (Enum.IsDefined(typeof(T), value))
            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        return default(T);
    }

    public static float ParseFloat(string str)
    {
        return float.Parse(str, CultureInfo.InvariantCulture);
    }

    public static bool IsGray(this Color color)
    {
        return color.r == color.g && color.r == color.b && color.a == 1f;
    }

    public static T GetRandomItem<T>(this List<T> list)
    {
        int index = UnityEngine.Random.Range(0, list.Count);
        return list[index];
    }

    public static PlayerInfo GetPlayerInfo(this PhotonPlayer player)
    {
        if (InGameManager.AllPlayerInfo.ContainsKey(player.ID))
            return InGameManager.AllPlayerInfo[player.ID];
        return null;
    }
}
