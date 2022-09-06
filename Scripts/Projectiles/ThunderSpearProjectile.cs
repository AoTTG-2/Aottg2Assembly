using Settings;
using UnityEngine;
using Photon;
using Characters;
using System.Collections.Generic;
using System.Collections;
using Effects;
using ApplicationManagers;
using GameManagers;

namespace Projectiles
{
    class ThunderSpearProjectile: BaseProjectile
    {
        Color _color;
        float _radius;

        public override void Setup(float liveTime, Vector3 velocity, int charViewId, object[] settings)
        {
            base.Setup(liveTime, velocity, charViewId, settings);
            _radius = (float)settings[0];
            _color = (Color)settings[1];
        }

        protected override void RegisterObjects()
        {
            var trail = transform.Find("Trail").GetComponent<ParticleSystem>();
            var flame = transform.Find("Flame").GetComponent<ParticleSystem>();
            var model = transform.Find("ThunderSpearModel").gameObject;
            _colliders.Add(GetComponent<SphereCollider>());
            _hideObjects.Add(flame.gameObject);
            _hideObjects.Add(model);
            if (SettingsManager.AbilitySettings.ShowBombColors.Value)
            {
                trail.startColor = _color;
                flame.startColor = _color;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (photonView.isMine && !Disabled)
                Explode();
        }

        public void Explode()
        {
            if (!Disabled)
            {
                EffectSpawner.Spawn(EffectPrefabs.ThunderSpearExplode, transform.position, transform.rotation,
                    new object[] { _radius, _color });
                KillPlayersInRadius(_radius);
                DestroySelf();
            }
        }

        void KillPlayersInRadius(float radius)
        {
            var gameManager = (InGameManager)SceneLoader.CurrentGameManager;
            var position = transform.position;
            foreach (Human human in gameManager.Humans)
            {
                if (Vector3.Distance(human.Cache.Transform.position, position) < radius && !human.IsMine() && !TeamInfo.SameTeam(human, _owner))
                {
                    human.GetHit(_owner, 100, "ThunderSpear");
                }
            }
        }
    }
}
