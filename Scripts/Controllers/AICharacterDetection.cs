using UnityEngine;
using System.Collections.Generic;
using Utility;
using Projectiles;
using GameManagers;

namespace Characters
{
    class AICharacterDetection: MonoBehaviour
    {
        public HashSet<BaseCharacter> Enemies = new HashSet<BaseCharacter>();
        public BaseCharacter Owner;

        public static AICharacterDetection Create(BaseCharacter owner, float radius)
        {
            GameObject go = new GameObject();
            go.transform.SetParent(owner.Cache.Transform);
            go.transform.localPosition = Vector3.zero;
            AICharacterDetection detection = go.AddComponent<AICharacterDetection>();
            go.layer = PhysicsLayer.CharacterDetection;
            SphereCollider collider = go.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = radius;
            detection.Owner = owner;
            return detection;
        }

        protected void OnTriggerEnter(Collider other)
        {
            GameObject obj = other.transform.root.gameObject;
            BaseCharacter character = obj.GetComponent<BaseCharacter>();
            if (character != null && !TeamInfo.SameTeam(character, Owner))
                Enemies.Add(character);
        }

        protected void OnTriggerExit(Collider other)
        {
            GameObject obj = other.transform.root.gameObject;
            BaseCharacter character = obj.GetComponent<BaseCharacter>();
            if (character != null && Enemies.Contains(character))
                Enemies.Remove(character);
        }

        protected void Update()
        {
            Util.RemoveNull(Enemies);
        }
    }
}
