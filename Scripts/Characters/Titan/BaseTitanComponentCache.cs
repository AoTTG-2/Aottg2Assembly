using UnityEngine;
using Utility;
using ApplicationManagers;
using System.Collections.Generic;

namespace Characters
{
    class BaseTitanComponentCache: BaseComponentCache
    {
        public Transform Head;
        public Transform Neck;
        public Collider NapeHurtbox;
        public Collider Movebox;
        public List<Collider> ToggleColliders = new List<Collider>();

        public BaseTitanComponentCache(GameObject owner): base(owner)
        {
            foreach (Collider c in owner.GetComponentsInChildren<Collider>())
            {
                GameObject go = c.gameObject;
                if (go.layer == PhysicsLayer.TitanPushbox || go.layer == PhysicsLayer.Hurtbox)
                    ToggleColliders.Add(c);
                else if (go.layer == PhysicsLayer.Hitbox)
                    c.enabled = false;
            }
        }
    }
}
