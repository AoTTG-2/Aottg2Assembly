using ApplicationManagers;

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Map
{
    class MapConverter
    {
        private static int _currentId = 0;
        public static bool IsLegacy(string map)
        {
            foreach (string str in map.Split(';'))
            {
                if (str.Trim() == string.Empty)
                    continue;
                if (str.StartsWith("///"))
                    return false;
                else
                    return true;
            }
            return false;
        }

        private static MapScriptSceneObject CreateForestFloor()
        {
            MapScriptSceneObject sceneObject = new MapScriptSceneObject();
            sceneObject.Asset = "Geometry/Cuboid";
            sceneObject.SetScale(new Vector3(134.29f, 6.41f, 139.29f));
            sceneObject.SetPosition(new Vector3(-7.8f, -32f, 5.3f));
            sceneObject.Id = GetNextId();
            var material = new MapScriptBasicMaterial();
            material.Shader = "Basic";
            material.Texture = BuiltinMapTextures.AllTextures["Grass1"].Texture;
            material.Tiling = new Vector2(50f, 50f);
            material.Color = new Color(0.678f, 0.684f, 0.654f, 1f);
            sceneObject.Material = material;
            return sceneObject;
        }

        private static MapScriptSceneObject CreateLight()
        {
            MapScriptSceneObject sceneObject = new MapScriptSceneObject();
            sceneObject.Copy(BuiltinMapPrefabs.AllPrefabs["Daylight"]);
            sceneObject.SetRotation(new Quaternion(-0.2f, -0.8f, 0.4f, -0.4f));
            sceneObject.Id = GetNextId();
            return sceneObject;
        }

        private static List<MapScriptBaseObject> CreateFengBounds()
        {
            var objects = new List<MapScriptBaseObject>();
            Vector3 rotation = new Vector3(0f, 0f, 0f);
            objects.Add(CreateBound(new Vector3(-700f, 745.8f, 0f), new Vector3(10f, 160f, 160f), rotation));
            objects.Add(CreateBound(new Vector3(0f, 745.8f, -700f), new Vector3(160f, 160f, 10f), rotation));
            objects.Add(CreateBound(new Vector3(0f, 745.8f, 700f), new Vector3(160, 160f, 10f), rotation));
            objects.Add(CreateBound(new Vector3(700f, 745.8f, 0f), new Vector3(10f, 160f, 160f), rotation));
            objects.Add(CreateBound(new Vector3(-2.2f, 1253.1f, 17.9f), new Vector3(160f, 10f, 160f), rotation));
            objects.Add(CreateBound(new Vector3(7.7f, 3039.1f, -78.9f), new Vector3(452.8f, 28.3f, 452.8f), rotation));
            objects.Add(CreateBound(new Vector3(11.2f, 942.2f, 2320f), new Vector3(452.8f, 452.8f, 28.3f), rotation));
            objects.Add(CreateBound(new Vector3(11.2f, 942.2f, -2555.7f), new Vector3(452.8f, 452.8f, 28.3f), rotation));
            objects.Add(CreateBound(new Vector3(-2389.3f, 942.2f, -107.2f), new Vector3(28.3f, 452.8f, 452.8f), rotation));
            objects.Add(CreateBound(new Vector3(2434.6f, 942.2f, -107.2f), new Vector3(28.3f, 452.8f, 452.8f), rotation));
            return objects;
        }

        private static List<MapScriptBaseObject> CreateRCBounds()
        {
            var objects = new List<MapScriptBaseObject>();
            objects.Add(CreateBound(new Vector3(-500f, 0f, 0f), new Vector3(1f, 1000f, 1000f), Vector3.zero));
            objects.Add(CreateBound(new Vector3(500f, 0f, 0f), new Vector3(1f, 1000f, 1000f), Vector3.zero));
            objects.Add(CreateBound(new Vector3(0f, 0f, -500f), new Vector3(1f, 1000f, 1000f), new Vector3(0f, 90f, 0f)));
            objects.Add(CreateBound(new Vector3(0f, 0f, 500f), new Vector3(1f, 1000f, 1000f), new Vector3(0f, 90f, 0f)));
            objects.Add(CreateBound(new Vector3(0f, 500f, 0f), new Vector3(1f, 1000f, 1000f), new Vector3(0f, 0f, 90f)));
            objects.Add(CreateBound(new Vector3(0f, -500f, 0f), new Vector3(1f, 1000f, 1000f), new Vector3(0f, 0f, 90f)));
            return objects;
        }

        private static MapScriptSceneObject CreateBound(Vector3 center, Vector3 size, Vector3 rotation)
        {
            var sceneObject = new MapScriptSceneObject();
            sceneObject.Copy(BuiltinMapPrefabs.AllPrefabs["Barrier"]);
            sceneObject.SetPosition(center);
            sceneObject.SetScale(size);
            sceneObject.SetRotation(rotation);
            sceneObject.Id = GetNextId();
            return sceneObject;
        }

        public static MapScript Convert(string map)
        {
            _currentId = 0;
            MapScript mapScript = new MapScript();
            mapScript.Objects.Objects.Add(CreateForestFloor());
            mapScript.Objects.Objects.Add(CreateLight());
            bool disableBounds = false;
            foreach (string str in map.Split(';'))
            {
                string[] strArray = str.Trim().Split(',');
                if (strArray.Length == 0)
                    continue;
                MapScriptSceneObject sceneObject = new MapScriptSceneObject();
                if (strArray[0].StartsWith("custom"))
                {
                    if (BuiltinMapPrefabs.AllPrefabsLower.ContainsKey(strArray[1].ToLower()))
                        sceneObject.Asset = BuiltinMapPrefabs.AllPrefabsLower[strArray[1].ToLower()].Asset;
                    sceneObject.SetPosition(new Vector3(float.Parse(strArray[12]), float.Parse(strArray[13]), float.Parse(strArray[14])));
                    sceneObject.SetRotation(new Quaternion(float.Parse(strArray[15]), float.Parse(strArray[16]), float.Parse(strArray[17]), float.Parse(strArray[18])));
                    sceneObject.SetScale(new Vector3(float.Parse(strArray[3]), float.Parse(strArray[4]), float.Parse(strArray[5])));
                    string texture = strArray[2];
                    MapScriptBaseMaterial material = new MapScriptBaseMaterial();
                    if (texture != "default")
                    {
                        material = new MapScriptBasicMaterial();
                        material.Shader = "Basic";
                        if (texture == "bark" || texture == "grass")
                            texture += "1";
                        if (BuiltinMapTextures.AllTexturesLower.ContainsKey(texture))
                        {
                            var newMaterial = BuiltinMapTextures.AllTexturesLower[texture];
                            ((MapScriptBasicMaterial)material).Texture = newMaterial.Texture;
                            ((MapScriptBasicMaterial)material).Tiling = new Vector2(float.Parse(strArray[10]), float.Parse(strArray[11]));
                        }
                        else
                            Debug.Log("Unhandled legacy texture: " + str);
                    }
                    if (strArray[6] != "0")
                    {
                        material.Color = new Color(float.Parse(strArray[7]), float.Parse(strArray[8]), float.Parse(strArray[9]), 1f);
                    }
                    sceneObject.Material = material;
                }
                else if (strArray[0].StartsWith("spawnpoint"))
                {
                    if (strArray[1] == "titan")
                        sceneObject.Copy(BuiltinMapPrefabs.AllPrefabs["Titan SpawnPoint"]);
                    else if (strArray[1] == "player")
                        sceneObject.Copy(BuiltinMapPrefabs.AllPrefabs["Human SpawnPoint"]);
                    else if (strArray[1] == "playerC")
                        sceneObject.Copy(BuiltinMapPrefabs.AllPrefabs["Human SpawnPoint (blue)"]);
                    else if (strArray[1] == "playerM")
                        sceneObject.Copy(BuiltinMapPrefabs.AllPrefabs["Human SpawnPoint (red)"]);
                    sceneObject.SetPosition(new Vector3(float.Parse(strArray[2]), float.Parse(strArray[3]), float.Parse(strArray[4])));
                }
                else if (strArray[0].StartsWith("misc"))
                {
                    if (strArray[1] == "barrier")
                        sceneObject.Copy(BuiltinMapPrefabs.AllPrefabs["Barrier"]);
                    else if (strArray[1] == "barrierEditor")
                    {
                        sceneObject.Asset = "Geometry/Cuboid";
                        sceneObject.Material = new MapScriptBasicMaterial
                        {
                            Shader = "Transparent",
                            Color = new Color(0f, 0.917f, 1f, 0.32f)
                        };
                    }
                    else if (strArray[1] == "racingStart")
                        sceneObject.Copy(BuiltinMapPrefabs.AllPrefabs["Racing Start Barrier"]);
                    else if (strArray[1] == "racingEnd")
                        sceneObject.Copy(BuiltinMapPrefabs.AllPrefabs["Racing End Region"]);
                    sceneObject.SetPosition(new Vector3(float.Parse(strArray[5]), float.Parse(strArray[6]), float.Parse(strArray[7])));
                    sceneObject.SetRotation(new Quaternion(float.Parse(strArray[8]), float.Parse(strArray[9]), float.Parse(strArray[10]), float.Parse(strArray[11])));
                    sceneObject.SetScale(new Vector3(float.Parse(strArray[2]), float.Parse(strArray[3]), float.Parse(strArray[4])));
                }
                else if (strArray[0].StartsWith("base"))
                {
                    Vector3 offset = Vector3.zero;
                    if (strArray[1] == "aot_supply")
                    {
                        sceneObject.Copy(BuiltinMapPrefabs.AllPrefabs["Supply1"]);
                        offset = Vector3.up * 3.1f;
                    }
                    if (strArray.Length < 15)
                    {
                        sceneObject.SetPosition(new Vector3(float.Parse(strArray[2]), float.Parse(strArray[3]), float.Parse(strArray[4])) + offset);
                        sceneObject.SetRotation(new Quaternion(float.Parse(strArray[5]), float.Parse(strArray[6]), float.Parse(strArray[7]), float.Parse(strArray[8])));
                    }
                    else
                    {
                        sceneObject.SetPosition(new Vector3(float.Parse(strArray[12]), float.Parse(strArray[13]), float.Parse(strArray[14])) + offset);
                        sceneObject.SetRotation(new Quaternion(float.Parse(strArray[15]), float.Parse(strArray[16]), float.Parse(strArray[17]), float.Parse(strArray[18])));
                        sceneObject.SetScale(new Vector3(float.Parse(strArray[3]), float.Parse(strArray[4]), float.Parse(strArray[5])));
                    }
                }
                else if (strArray[0].StartsWith("map"))
                {
                    if (strArray[1].StartsWith("disablebounds"))
                    {
                        disableBounds = true;
                        continue;
                    }
                }
                if (sceneObject.Asset == "None" && sceneObject.Name == "Unnamed")
                    Debug.Log("Unhandled legacy object: " + str);
                else
                {
                    sceneObject.Id = GetNextId();
                    mapScript.Objects.Objects.Add(sceneObject);
                }
            }
            if (disableBounds)
                mapScript.Objects.Objects.AddRange(CreateRCBounds());
            else
                mapScript.Objects.Objects.AddRange(CreateFengBounds());
            return mapScript;
        }

        private static int GetNextId()
        {
            _currentId++;
            return _currentId;
        }
    }
}
