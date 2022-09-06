using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Utility;

namespace Map
{
    class MapScriptBasicMaterial: MapScriptBaseMaterial
    {
        [Order(3)] public string Texture = "None";
        [Order(4)] public Vector2 Tiling = Vector2.one;
        [Order(5)] public Vector2 Offset = Vector3.zero;

        public override void Deserialize(string csv)
        {
            string[] items = csv.Split(Delimiter);
            List<FieldInfo> fields = GetFields();
            for (int i = 0; i < items.Length; i++)
                items[i] = items[i].Trim();
            if (NamedParams)
            {
                foreach (string item in items)
                {
                    string[] paramItems = item.Split(ParamDelimiter);
                    FieldInfo field = FindField(paramItems[0]);
                    if (field != null)
                        DeserializeField(field, this, paramItems[1]);
                }
            }
            else
            {
                for (int i = 0; i < fields.Count; i++)
                {
                    if (IsList(fields[i]))
                    {
                        Type t = fields[i].FieldType.GetGenericArguments()[0];
                        List<object> list = (List<object>)fields[i].GetValue(this);
                        list.Clear();
                        for (int j = i; j < items.Length; i++)
                            list.Add(DeserializeValue(t, items[j]));
                        break;
                    }
                    else
                        DeserializeField(fields[i], this, items[i]);
                }
            }
        }
    }
}
