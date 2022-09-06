using Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Utility
{
    public static class Util
    {
        public static void DontDestroyOnLoad(GameObject obj)
        {
            UnityEngine.Object.DontDestroyOnLoad(obj);
            obj.AddComponent<DontDestroyOnLoadTag>();
        }

        public static T CreateDontDestroyObj<T>() where T : Component
        {
            GameObject obj = new GameObject();
            Util.DontDestroyOnLoad(obj);
            return obj.AddComponent<T>();
        }

        public static void RemoveNull<T>(HashSet<T> set)
        {
            if (set.Count == 0)
                return;
            List<T> remove = new List<T>();
            foreach (T item in set)
            {
                if (item == null)
                    remove.Add(item);
            }
            foreach (T item in remove)
                set.Remove(item);
        }

        public static string CreateMD5(string input)
        {
            if (input == string.Empty)
                return string.Empty;
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public static IEnumerator WaitForFrames(int frames)
        {
            for (int i = 0; i < frames; i++)
                yield return new WaitForEndOfFrame();
        }

        public static string[] EnumToStringArray<T>()
        {
            return Enum.GetNames(typeof(T));
        }

        public static string[] EnumToStringArrayExceptNone<T>()
        {
            List<string> names = new List<string>();
            foreach (string str in EnumToStringArray<T>())
            {
                if (str != "None")
                    names.Add(str);
            }
            return names.ToArray();
        }

        public static List<T> EnumToList<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToList();
        }

        public static Dictionary<string, T> EnumToDict<T>()
        {
            Dictionary<string, T> dict = new Dictionary<string, T>();
            foreach (T t in EnumToList<T>())
            {
                dict.Add(t.ToString(), t);
            }
            return dict;
        }

        public static string FormatFloat(float num, int decimalPlaces)
        {
            if (decimalPlaces == 0)
                return num.ToString("0");
            return num.ToString("0." + new string('0', decimalPlaces));
        }

        public static Vector3 MultiplyVectors(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public static Vector3 DivideVectors(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }
    }

    class DontDestroyOnLoadTag : MonoBehaviour
    {
    }
}