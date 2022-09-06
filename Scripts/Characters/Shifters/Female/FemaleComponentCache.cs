using UnityEngine;
using Utility;
using ApplicationManagers;

namespace Characters
{
    class FemaleComponentCache: ShifterComponentCache
    {
        public BaseHitbox HandLHitbox;
        public BaseHitbox HandRHitbox;
        public BaseHitbox ShinLHitbox;
        public BaseHitbox ShinRHitbox;

        public FemaleComponentCache(GameObject owner): base(owner)
        {
            Head = Transform.Find("Armature_FemT/Core/Controller_Body/hip/spine/chest/neck/head");
            Neck = Transform.Find("Armature_FemT/Core/Controller_Body/hip/spine/chest/neck");
            NapeHurtbox = Transform.Find("Armature_FemT/Core/Controller_Body/hip/spine/chest/neck/NapeHurt").GetComponent<Collider>();
            Movebox = owner.GetComponent<CapsuleCollider>();
            BaseCharacter character = owner.GetComponent<BaseCharacter>();
            foreach (Collider collider in owner.GetComponentsInChildren<Collider>())
            {
                string name = collider.gameObject.name;
                if (name == "HandLHit")
                    HandLHitbox = BaseHitbox.Create(character, collider.gameObject, collider);
                else if (name == "HandRHit")
                    HandRHitbox = BaseHitbox.Create(character, collider.gameObject, collider);
                else if (name == "ShinLHit")
                    ShinLHitbox = BaseHitbox.Create(character, collider.gameObject, collider);
                else if (name == "ShinRHit")
                    ShinRHitbox = BaseHitbox.Create(character, collider.gameObject, collider);
            }
        }
    }
}
