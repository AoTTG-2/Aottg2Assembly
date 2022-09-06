using UnityEngine;
using System.Collections.Generic;
using Utility;
using GameManagers;
using System.Collections;

namespace Characters
{
    class BaseHitbox: MonoBehaviour
    {
        public BaseCharacter Owner;
        public bool OnEnter = true;
        protected HashSet<BaseCharacter> _hitCharacters = new HashSet<BaseCharacter>();
        protected Collider _collider;

        public static BaseHitbox Create(BaseCharacter owner, GameObject obj, Collider collider = null)
        {
            BaseHitbox hitbox = obj.AddComponent<BaseHitbox>();
            hitbox.Owner = owner;
            if (collider == null)
                collider = obj.GetComponent<Collider>();
            hitbox._collider = collider;
            hitbox.Deactivate();
            return hitbox;
        }

        public bool IsActive()
        {
            return _collider.enabled;
        }
        
        public void Activate(float delay = 0f, float length = 0f)
        {
            _hitCharacters.Clear();
            if (delay == 0f)
                _collider.enabled = true;
            else
                StartCoroutine(WaitAndActivate(delay));
            if (length > 0f)
                StartCoroutine(WaitAndDeactivate(delay + length));
        }

        public void Deactivate()
        {
            _collider.enabled = false;
        }

        protected IEnumerator WaitAndActivate(float delay)
        {
            yield return new WaitForSeconds(delay);
            _collider.enabled = true;
        }

        protected IEnumerator WaitAndDeactivate(float delay)
        {
            yield return new WaitForSeconds(delay);
            _collider.enabled = false;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!OnEnter)
                return;
            OnTrigger(other);
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            if (OnEnter)
                return;
            OnTrigger(other);
        }

        protected virtual void OnTrigger(Collider other)
        {
            var go = other.transform.root.gameObject;
            BaseCharacter character = go.GetComponent<BaseCharacter>();
            if (character != null && !TeamInfo.SameTeam(Owner, character) && !_hitCharacters.Contains(character))
            {
                _hitCharacters.Add(character);
                OnHit(character, other);
            }
        }

        protected virtual void OnHit(BaseCharacter victim, Collider collider)
        {
            Owner.OnHit(this, victim, collider);
        }
    }
}
