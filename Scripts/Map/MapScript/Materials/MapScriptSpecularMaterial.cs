using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace Map
{
    class MapScriptSpecularMaterial: MapScriptBasicMaterial
    {
        [Order(3)] public Color SpecularColor = Color.white;
        [Order(4)] public float Shininess = 0.5f;
    }
}
