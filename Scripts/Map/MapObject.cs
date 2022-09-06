using UnityEngine;

namespace Map
{
    class MapObject
    {
        public int Parent;
        public GameObject GameObject;
        public MapScriptBaseObject ScriptObject;
        public Vector3 BaseScale;

        public MapObject(int parent, GameObject gameObject, MapScriptBaseObject scriptObject, Vector3 baseScale)
        {
            Parent = parent;
            GameObject = gameObject;
            ScriptObject = scriptObject;
            BaseScale = baseScale;
        }
    }
}
