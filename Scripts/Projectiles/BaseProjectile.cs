using Settings;
using UnityEngine;
using Photon;
using Characters;
using System.Collections.Generic;
using System.Collections;

namespace Projectiles
{
    class BaseProjectile: BaseMovementSync
    {
        protected BaseCharacter _owner;
        protected float _timeLeft;
        protected List<GameObject> _hideObjects = new List<GameObject>();
        protected List<Collider> _colliders = new List<Collider>();
        protected List<ParticleSystem> _fadeTrails = new List<ParticleSystem>();
        protected float TrailFadeMultiplier => 0.6f;
        protected float DestroyDelay => 1.5f;

        public virtual void Setup(float liveTime, Vector3 velocity, int charViewId, object[] settings)
        {
            photonView.observed = this;
            _timeLeft = liveTime;
            _rigidbody.velocity = velocity;
            if (charViewId != -1)
                _owner = PhotonView.Find(charViewId).GetComponent<BaseCharacter>();
            RegisterObjects();
            if (_owner != null)
            {
                foreach (Collider c1 in _owner.Cache.Colliders)
                {
                    foreach (Collider c2 in _colliders)
                    {
                        if (c1.enabled && c2.enabled)
                            Physics.IgnoreCollision(c1, c2);
                    }
                }
            }
        }

        public bool IsMine()
        {
            return photonView.isMine;
        }

        protected override void Update()
        {
            base.Update();
            if (_photonView.isMine)
            {
                _timeLeft -= Time.deltaTime;
                if (_timeLeft <= 0f)
                    DestroySelf();
            }
        }

        protected virtual void RegisterObjects()
        {

        }

        public virtual void DestroySelf()
        {
            if (photonView.isMine && !Disabled)
            {
                photonView.RPC("DisableRPC", PhotonTargets.All, new object[0]);
                StartCoroutine(WaitAndFinishDestroyCoroutine(DestroyDelay));
            }
        }

        protected virtual IEnumerator WaitAndFinishDestroyCoroutine(float time)
        {
            yield return new WaitForSeconds(time);
            PhotonNetwork.Destroy(gameObject);
        }

        [RPC]
        public virtual void DisableRPC(PhotonMessageInfo info = null)
        {
            if (Disabled)
                return;
            if (info != null && info.sender != photonView.owner)
                return;
            foreach (GameObject obj in _hideObjects)
                obj.SetActive(false);
            foreach (Collider c in _colliders)
                c.enabled = false;
            foreach (ParticleSystem system in _fadeTrails)
                SetDisabledTrailFade(system);
            rigidbody.velocity = Vector3.zero;
            Disabled = true;
        }

        protected void SetDisabledTrailFade(ParticleSystem particleSystem)
        {
            int particleCount = particleSystem.particleCount;
            float newLifetime = particleSystem.startLifetime * TrailFadeMultiplier;
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleCount];
            particleSystem.GetParticles(particles);
            for (int i = 0; i < particleCount; i++)
            {
                particles[i].lifetime *= TrailFadeMultiplier;
                particles[i].startLifetime = newLifetime;
            }
            particleSystem.SetParticles(particles, particleCount);
        }

    }
}
