using System;
using UnityEngine;
using ApplicationManagers;
using GameManagers;
using UnityEngine.UI;
using Utility;
using System.Collections.Generic;
using Settings;
using System.Collections;

namespace Characters
{
    class BaseCharacter: Photon.MonoBehaviour
    {
        protected virtual float DefaultMaxHealth => 1f;
        protected virtual bool HasMovement => true;
        protected virtual Vector3 Gravity => Vector3.down * 20f;
        public virtual List<string> EmoteActions => new List<string>();

        // setup
        public BaseComponentCache Cache;
        public bool AI;
        public float MaxHealth;
        public float CurrentHealth;
        public string Team;
        protected Text _nameLabel;
        // protected HealthBarPopup _healthBar;
        protected InGameManager _inGameManager;
        protected BaseMovementSync _movementSync;

        // movement
        public bool Grounded;
        public bool JustGrounded;
        public float TargetAngle;
        public bool HasDirection;
        protected virtual LayerMask GroundMask => PhysicsLayer.GetMask(PhysicsLayer.TitanMovebox, PhysicsLayer.MapObjectEntities,
                PhysicsLayer.MapObjectCharacters, PhysicsLayer.MapObjectAll);
        protected virtual float GroundDistance => 0.3f;

        public bool IsMine()
        {
            return Cache.PhotonView.isMine;
        }

        public bool IsMainCharacter()
        {
            return _inGameManager.CurrentCharacter == this;
        }

        public virtual void Init(bool ai, string team)
        {
            AI = ai;
            Team = team;
        }

        public virtual Transform GetCameraAnchor()
        {
            return Cache.Transform;
        }

        protected virtual void CreateCache(BaseComponentCache cache)
        {
            Cache = cache;
            if (cache == null)
                Cache = new BaseComponentCache(gameObject);
        }

        public virtual void Emote(string emote)
        {
        }

        [RPC]
        public void SetHealthRPC(float currentHealth, float maxHealth, PhotonMessageInfo info)
        {
            if (info.sender == photonView.owner)
            {
                CurrentHealth = currentHealth;
                MaxHealth = maxHealth;
            }
        }

        public void SetCurrentHealth(float currentHealth)
        {
            CurrentHealth = Mathf.Min(currentHealth, MaxHealth);
            if (CurrentHealth <= 0f)
                Die();
            else
                OnHealthChange();
        }

        public void SetMaxHealth(float maxHealth)
        {
            MaxHealth = maxHealth;
            SetCurrentHealth(CurrentHealth);
        }

        public virtual void TakeDamage(float damage)
        {
            SetCurrentHealth(CurrentHealth - damage);
        }

        public virtual void Die()
        {
            StartCoroutine(WaitAndDie());
        }

        protected virtual IEnumerator WaitAndDie()
        {
            PhotonNetwork.Destroy(gameObject);
            yield break;
        }

        public void PlayAnimation(string animation, float startTime = 0f)
        {

            Cache.PhotonView.RPC("PlayAnimationRPC", PhotonTargets.All, new object[] { animation, startTime });
        }

        [RPC]
        public void PlayAnimationRPC(string animation, float startTime, PhotonMessageInfo info)
        {
            if (info.sender != Cache.PhotonView.owner)
                return;
            Cache.Animation.Play(animation);
            Cache.Animation[animation].normalizedTime = startTime;
        }

        public void CrossFade(string animation, float fadeTime = 0f, float startTime = 0f)
        {
            Cache.PhotonView.RPC("CrossFadeRPC", PhotonTargets.All, new object[] { animation, fadeTime, startTime });
        }

        [RPC]
        public void CrossFadeRPC(string animation, float fadeTime, float startTime, PhotonMessageInfo info)
        {
            if (info.sender != Cache.PhotonView.owner)
                return;
            Cache.Animation.CrossFade(animation, fadeTime);
            Cache.Animation[animation].normalizedTime = startTime;
        }

        public void PlaySound(string sound)
        {
            Cache.PhotonView.RPC("PlaySoundRPC", PhotonTargets.All, new object[] { sound });
        }

        [RPC]
        public void PlaySoundRPC(string sound, PhotonMessageInfo info = null)
        {
            if (info != null && info.sender != Cache.PhotonView.owner)
                return;
            if (Cache.AudioSources.ContainsKey(sound))
                Cache.AudioSources[sound].Play();
        }

        public void StopSound(string sound)
        {
            Cache.PhotonView.RPC("StopSound", PhotonTargets.All, new object[] { sound });
        }

        [RPC]
        public void StopSoundRPC(string sound, PhotonMessageInfo info = null)
        {
            if (info != null && info.sender != Cache.PhotonView.owner)
                return;
            if (Cache.AudioSources.ContainsKey(sound))
                Cache.AudioSources[sound].Stop();
        }

        protected void SetupHealthBar()
        {

        }

        protected void SetupNameLabel()
        {

        }


        protected virtual void OnHealthChange()
        {
            if (IsMine())
                photonView.RPC("SetHealthRPC", PhotonTargets.All, new object[] { CurrentHealth, MaxHealth });
        }

        public virtual void OnHit(BaseHitbox hitbox, BaseCharacter victim, Collider collider)
        {
        }

        [RPC]
        public virtual void GetHitRPC(int viewId, int damage, string type)
        {
            TakeDamage(damage);
        }

        public virtual void GetHit(BaseCharacter enemy, int damage, string type = "")
        {
            int viewId = -1;
            if (enemy != null)
                viewId = enemy.Cache.PhotonView.viewID;
            Cache.PhotonView.RPC("GetHitRPC", Cache.PhotonView.owner, new object[] { viewId, damage, type });
        }


        protected virtual void Awake()
        {
            if (SceneLoader.CurrentGameManager is InGameManager)
                _inGameManager = (InGameManager)SceneLoader.CurrentGameManager;
            if (HasMovement)
                _movementSync = gameObject.AddComponent<BaseMovementSync>();
            CreateCache(null);
            SetColliders();
        }

        protected virtual void SetColliders()
        {
        }

        protected virtual void Start()
        {
            CurrentHealth = MaxHealth = DefaultMaxHealth;
            OnHealthChange();
        }

        protected virtual Quaternion GetTargetRotation()
        {
            return Quaternion.Euler(0f, TargetAngle, 0f);
        }

        protected virtual Vector3 GetTargetDirection()
        {
            float angleRadians = (90f - TargetAngle) * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(angleRadians), 0f, Mathf.Sin(angleRadians)).normalized;
        }

        protected virtual void CheckGround()
        {
            
            JustGrounded = false;
            if (Physics.Raycast(Cache.Transform.position + Vector3.up * 0.1f, -Vector3.up, GroundDistance, GroundMask.value))
            {
                if (!Grounded)
                    Grounded = JustGrounded = true;
            }
            else
                Grounded = false;
        }
    }
}
