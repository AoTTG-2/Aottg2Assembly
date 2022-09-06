using ApplicationManagers;
using GameManagers;
using Settings;
using UnityEngine;
using Utility;

namespace Effects
{
    class EffectSpawner
    {
        public static void Spawn(string name, Vector3 position, Quaternion rotation, object[] settings = null)
        {
            RPCManager.PhotonView.RPC("SpawnEffectRPC", PhotonTargets.All, new object[] { name, position, rotation, settings });
        }

        public static void OnSpawnEffectRPC(string name, Vector3 position, Quaternion rotation, object[] settings, PhotonMessageInfo info)
        {
            GameObject go;
            if (name.StartsWith("RCAsset/"))
                go = AssetBundleManager.InstantiateAsset<GameObject>(name.Substring(8), position, rotation);
            else
                go = ResourceManager.InstantiateAsset<GameObject>(name, position, rotation);
            BaseEffect effect;
            if (name == EffectPrefabs.ThunderSpearExplode)
            {
                effect = go.AddComponent<ThunderSpearExplodeEffect>();
                effect.Setup(info.sender, 5f, settings);
            }
            else
            {
                effect = go.AddComponent<BaseEffect>();
                effect.Setup(info.sender, 5f, settings);
            }
        }
    }
}
