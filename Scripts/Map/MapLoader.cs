using ApplicationManagers;
using Events;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using Utility;

namespace Map
{
    class MapLoader: MonoBehaviour
    {
        public static Dictionary<int, MapObject> IdToMapObject = new Dictionary<int, MapObject>();
        public static Dictionary<int, List<int>> IdToChildren = new Dictionary<int, List<int>>();
        public static Dictionary<GameObject, MapObject> GoToMapObject = new Dictionary<GameObject, MapObject>();
        public static Dictionary<string, List<MapObject>> Tags = new Dictionary<string, List<MapObject>>();
        public static List<Light> Daylight = new List<Light>();
        private static Dictionary<string, Object> _assetCache = new Dictionary<string, Object>();
        private static Dictionary<string, List<Material>> _defaultMaterialCache = new Dictionary<string, List<Material>>();
        private static Dictionary<string, Material> _customMaterialCache = new Dictionary<string, Material>();
        private static MapLoader _instance;

        public static void Init()
        {
            _instance = SingletonFactory.CreateSingleton(_instance);
            EventManager.OnPreLoadScene += OnPreLoadScene;
        }

        private static void OnPreLoadScene(SceneName sceneName)
        {
            _instance.StopAllCoroutines();
        }

        public static void StartLoadObjects(List<MapScriptBaseObject> objects, bool editor = false)
        {
            _customMaterialCache.Clear();
            _defaultMaterialCache.Clear();
            IdToChildren.Clear();
            IdToMapObject.Clear();
            GoToMapObject.Clear();
            Daylight.Clear();
            _assetCache.Clear();
            Tags.Clear();
            _instance.StartCoroutine(_instance.LoadObjectsCoroutine(objects, editor));
        }

        public static MapObject FindObjectFromCollider(Collider collider)
        {
            Transform current = collider.transform;
            while (true)
            {
                var go = current.gameObject;
                if (GoToMapObject.ContainsKey(go))
                    return GoToMapObject[go];
                if (current.parent == null)
                    return null;
                current = current.parent;
            }
        }

        public static MapObject LoadObject(MapScriptBaseObject scriptObject, bool editor, bool setTransform = true)
        {
            GameObject go = null;
            if (scriptObject is MapScriptSceneObject)
                go = LoadSceneObject((MapScriptSceneObject)scriptObject, editor);
            MapObject mapObject = new MapObject(scriptObject.Parent, go, scriptObject, go.transform.localScale);
            if (IdToMapObject.ContainsKey(scriptObject.Id))
            {
                DebugConsole.Log("Error: Map object with duplicate ID found (" + scriptObject.Id.ToString() + ")", true);
                return mapObject;
            }
            IdToMapObject.Add(scriptObject.Id, mapObject);
            GoToMapObject.Add(go, mapObject);
            if (scriptObject.Parent >= 0)
            {
                if (IdToChildren.ContainsKey(scriptObject.Parent))
                    IdToChildren[scriptObject.Parent].Add(scriptObject.Id);
                else
                    IdToChildren.Add(scriptObject.Parent, new List<int>() { scriptObject.Id });
            }
            if (setTransform)
                SetTransform(mapObject);
            return mapObject;
        }

        public static void DeleteObject(MapObject obj)
        {
            int id = obj.ScriptObject.Id;
            DeleteObject(id);
        }

        public static void DeleteObject(int id)
        {
            var mapObject = IdToMapObject[id];
            if (IdToChildren.ContainsKey(id))
            {
                foreach (int child in IdToChildren[id])
                    DeleteObject(child);
            }
            IdToChildren.Remove(id);
            IdToMapObject.Remove(id);
            GoToMapObject.Remove(mapObject.GameObject);
            Destroy(mapObject.GameObject);
        }

        private IEnumerator LoadObjectsCoroutine(List<MapScriptBaseObject> objects, bool editor)
        {
            int count = 0;
            foreach (MapScriptBaseObject obj in objects)
            {
                LoadObject(obj, editor, false);
                if (count % 100 == 0 && SceneLoader.SceneName == SceneName.InGame)
                {
                    ((InGameMenu)UIManager.CurrentMenu).UpdateLoading(0.9f + 0.1f * (count / objects.Count));
                    yield return new WaitForEndOfFrame();
                }
                count++;
            }
            foreach (int id in IdToMapObject.Keys)
            {
                var mapObject = IdToMapObject[id];
                SetTransform(mapObject);
            }
            if (!editor)
                Batch();
            MapManager.MapLoaded = true;
        }

        private void Batch()
        {
            Dictionary<string, GameObject> roots = new Dictionary<string, GameObject>();
            Dictionary<string, List<GameObject>> shared = new Dictionary<string, List<GameObject>>();
            Dictionary<GameObject, Transform> oldParents = new Dictionary<GameObject, Transform>();
            foreach (int id in IdToMapObject.Keys)
            {
                var mapObject = IdToMapObject[id];
                if (mapObject.ScriptObject.Parent >= 0 || !mapObject.ScriptObject.Static)
                    continue;
                foreach (MeshFilter filter in mapObject.GameObject.GetComponentsInChildren<MeshFilter>())
                {
                    string hash = filter.sharedMesh.GetHashCode().ToString() + "/";
                    if (filter.renderer.enabled)
                        hash += filter.renderer.sharedMaterial.GetHashCode().ToString();
                    else
                        hash += "disabled";
                    if (!roots.ContainsKey(hash))
                    {
                        roots.Add(hash, new GameObject());
                        shared.Add(hash, new List<GameObject>());
                    }
                    shared[hash].Add(filter.gameObject);
                    oldParents.Add(filter.gameObject, filter.transform.parent);
                    filter.transform.SetParent(roots[hash].transform);
                }
            }
            foreach (string hash in roots.Keys)
                CombineMeshes(roots[hash]);
            foreach (string hash in shared.Keys)
            {
                foreach (GameObject go in shared[hash])
                    go.transform.SetParent(oldParents[go]);
            }
        }

        void CombineMeshes(GameObject obj)
        {
            MeshFilter[] meshFilters = obj.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];
            if (meshFilters.Length == 0)
                return;
            bool rendererEnabled = meshFilters[0].renderer.enabled;
            for (int i = 0; i < meshFilters.Length; i++)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                meshFilters[i].renderer.enabled = false;
            }
            obj.AddComponent<MeshFilter>();
            obj.AddComponent<MeshRenderer>();
            obj.transform.GetComponent<MeshFilter>().mesh = new Mesh();
            obj.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true, true);
            if (rendererEnabled)
                obj.GetComponent<Renderer>().material = meshFilters[0].GetComponent<Renderer>().material;
            else
                obj.GetComponent<Renderer>().enabled = false;
        }

        public static void RegisterTag(string tag, MapObject obj)
        {
            if (!Tags.ContainsKey(tag))
                Tags.Add(tag, new List<MapObject>());
            Tags[tag].Add(obj);
        }

        private static GameObject LoadSceneObject(MapScriptSceneObject obj, bool editor)
        {
            GameObject go;
            if (obj.Asset == "None")
                go = new GameObject();
            else
                go = (GameObject)Instantiate(LoadAssetCached(obj.Asset.Split('/')[1]));
            if (editor)
                SetPhysics(go, MapObjectCollideMode.Physical, MapObjectCollideWith.MapEditor, obj.PhysicsMaterial);
            else
                SetPhysics(go, obj.CollideMode, obj.CollideWith, obj.PhysicsMaterial);
            SetMaterial(go, obj.Asset, obj.Material, obj.Visible);
            return go;
        }

        private static void SetTransform(MapObject mapObject)
        {
            var go = mapObject.GameObject;
            var obj = mapObject.ScriptObject;
            Transform t = go.transform;
            go.isStatic = obj.Static;
            go.name = obj.Name;
            if (obj.Parent >= 0)
            {
                if (IdToMapObject.ContainsKey(obj.Parent))
                    t.SetParent(IdToMapObject[obj.Parent].GameObject.transform);
                else
                    DebugConsole.Log("Error: object parent id not found (" + obj.Parent.ToString() + ")", true);
            }
            t.localPosition = new Vector3(obj.PositionX, obj.PositionY, obj.PositionZ);
            t.localRotation = Quaternion.Euler(obj.RotationX, obj.RotationY, obj.RotationZ);
            t.localScale = new Vector3(mapObject.BaseScale.x * obj.ScaleX, mapObject.BaseScale.y * obj.ScaleY, mapObject.BaseScale.z * obj.ScaleZ);
        }

        private static void SetMaterial(GameObject go, string asset, MapScriptBaseMaterial material, bool visible)
        {
            if (material.Shader == MapObjectShader.Default.ToString())
            {
                string materialHash = asset + material.Serialize();
                Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
                if (_defaultMaterialCache.ContainsKey(materialHash))
                {
                    List<Material> mats = _defaultMaterialCache[materialHash];
                    for (int i = 0; i < renderers.Length; i++)
                    {
                        renderers[i].material = mats[i];
                        if (!visible)
                            renderers[i].enabled = false;
                    }
                }
                else
                {
                    List<Material> mats = new List<Material>();
                    foreach (Renderer renderer in renderers)
                    {
                        renderer.material.color = material.Color;
                        mats.Add(renderer.material);
                        if (!visible)
                            renderer.enabled = false;
                    }
                    _defaultMaterialCache.Add(materialHash, mats);
                }
            }
            else
            {
                Material mat = null;
                string materialHash = material.Serialize();
                if (_customMaterialCache.ContainsKey(materialHash))
                    mat = _customMaterialCache[materialHash];
                else
                {
                    if (material.GetType().IsAssignableFrom(typeof(MapScriptBasicMaterial)))
                    {
                        var basic = (MapScriptBasicMaterial)material;
                        mat = (Material)Instantiate(LoadAssetCached(basic.Shader + "Material"));
                        if (basic.Texture != "None")
                        {
                            var textureName = basic.Texture.Split('/')[1];
                            if (BuiltinMapTextures.AllTextures.ContainsKey(textureName))
                            {
                                var reference = BuiltinMapTextures.AllTextures[textureName];
                                var texture = (Texture2D)LoadAssetCached(textureName + "Texture");
                                mat.mainTexture = texture;
                                mat.mainTextureScale = new Vector2(basic.Tiling.x * reference.Tiling.x, basic.Tiling.y * reference.Tiling.y);
                                mat.mainTextureOffset = basic.Offset;
                                
                            }
                        }
                        mat.color = basic.Color;
                        if (material.Shader == MapObjectShader.Specular.ToString())
                        {
                            mat.SetFloat("_Shininess", ((MapScriptSpecularMaterial)material).Shininess);
                            mat.SetColor("_SpecularColor", ((MapScriptSpecularMaterial)material).Color);
                        }
                    }
                    _customMaterialCache.Add(materialHash, mat);
                }
                foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>())
                {
                    if (mat != null)
                        renderer.material = mat;
                    if (!visible)
                        renderer.enabled = false;
                }
            }
        }

        private static void SetPhysics(GameObject go, string collideMode, string collideWith, string physicsMaterial)
        {
            PhysicMaterial material = null;
            if (physicsMaterial != "Default")
                material = (PhysicMaterial)LoadAssetCached(physicsMaterial);
            int layer = 0;
            if (collideWith == MapObjectCollideWith.All)
                layer = PhysicsLayer.MapObjectAll;
            else if (collideWith == MapObjectCollideWith.MapObjects)
                layer = PhysicsLayer.MapObjectMapObjects;
            else if (collideWith == MapObjectCollideWith.Characters)
                layer = PhysicsLayer.MapObjectCharacters;
            else if (collideWith == MapObjectCollideWith.Projectiles)
                layer = PhysicsLayer.MapObjectProjectiles;
            else if (collideWith == MapObjectCollideWith.Entities)
                layer = PhysicsLayer.MapObjectEntities;
            else if (collideWith == MapObjectCollideWith.MapEditor)
                layer = PhysicsLayer.MapEditorObject;
            foreach (Collider c in go.GetComponentsInChildren<Collider>())
            {
                if (collideMode == MapObjectCollideMode.Region)
                    c.isTrigger = true;
                else if (collideMode == MapObjectCollideMode.None)
                    c.enabled = false;
                if (material != null)
                    c.material = material;
                c.gameObject.layer = layer;
            }
            go.layer = layer;
        }

        private static Object LoadAssetCached(string asset)
        {
            if (!_assetCache.ContainsKey(asset))
                _assetCache.Add(asset, AssetBundleManager.LoadAsset(asset));
            return _assetCache[asset];
        }
    }

    static class MapObjectShader
    {
        public static string Default = "Default";
        public static string Basic = "Basic";
        public static string Transparent = "Transparent";
        public static string Specular = "Specular";
    }

    static class MapObjectCollideMode
    {
        public static string Physical = "Physical";
        public static string Region = "Region";
        public static string None = "None";
    }

    static class MapObjectCollideWith
    {
        public static string All = "All";
        public static string MapObjects = "MapObjects";
        public static string Characters = "Characters";
        public static string Projectiles = "Projectiles";
        public static string Entities = "Entities";
        public static string MapEditor = "MapEditor";
    }
}
