using ApplicationManagers;
using GameManagers;
using System.Collections.Generic;
using UnityEngine;

namespace Projectiles
{
    class ProjectileSpawner: MonoBehaviour
    {
        public static BaseProjectile Spawn(string name, Vector3 position, Quaternion rotation, Vector3 velocity, int charViewId, object[] settings = null)
        {
            GameObject go = PhotonNetwork.Instantiate(name, position, rotation, 0);
            BaseProjectile projectile;
            switch (name)
            {
                default:
                    projectile = go.GetComponent<BaseProjectile>();
                    projectile.Setup(10f, velocity, charViewId, settings);
                    break;
            }
            return projectile;
        }
    }
}
