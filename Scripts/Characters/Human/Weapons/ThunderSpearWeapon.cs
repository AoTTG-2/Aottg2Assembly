using ApplicationManagers;
using Effects;
using Projectiles;
using Settings;
using UnityEngine;
using Utility;

namespace Characters
{
    class ThunderSpearWeapon : AmmoWeapon
    {
        public ThunderSpearProjectile Current;
        public float Radius;
        public float Speed;
        public float TravelTime;
        private float _travelTimeLeft;

        public ThunderSpearWeapon(BaseCharacter owner, int ammo, int ammoPerRound, float cooldown, float radius, float speed,
            float travelTime) : base(owner, ammo, ammoPerRound, cooldown)
        {
            Radius = radius;
            Speed = speed;
            TravelTime = travelTime;
        }

        protected override void Activate()
        {
            var human = (Human)_owner;
            Vector3 target = human.GetAimPoint();
            Vector3 direction = (target - human.Cache.Transform.position).normalized;
            float cross = Vector3.Cross(human.Cache.Transform.forward, direction).y;
            Vector3 spawnPosition;
            if (cross < 0f && human.State != HumanState.Land)
            {
                spawnPosition = human.Setup.ThunderSpearL.transform.position;
                human.Setup.ThunderSpearL.audio.Play();
                human.SetThunderSpears(false, true);
                human.AttackAnimation = "AHSS_shoot_l";
            }
            else
            {
                spawnPosition = human.Setup.ThunderSpearR.transform.position;
                human.Setup.ThunderSpearR.audio.Play();
                human.SetThunderSpears(true, false);
                human.AttackAnimation = "AHSS_shoot_r";
            }
            Vector3 spawnDirection = (target - spawnPosition).normalized;
            if (human.Grounded)
                spawnPosition += spawnDirection * 1f;
            if (human.State != HumanState.Slide)
            {
                if (human.State == HumanState.Attack)
                    human._attackButtonRelease = true;
                human.PlayAnimation(human.AttackAnimation, 0.1f);
                human.State = HumanState.Attack;
                human.TargetAngle = Quaternion.LookRotation(direction).eulerAngles.y;
                human._targetRotation = Quaternion.Euler(0f, human.TargetAngle, 0f);
            }
            Current = (ThunderSpearProjectile)ProjectileSpawner.Spawn(ProjectilePrefabs.ThunderSpear, spawnPosition, Quaternion.LookRotation(spawnDirection),
                spawnDirection * Speed, human.Cache.PhotonView.viewID, new object[] {Radius, SettingsManager.AbilitySettings.BombColor.Value});
            _travelTimeLeft = TravelTime;
        }

        public override void SetInput(bool key)
        {
            if (key)
            {
                if (Current != null && !Current.Disabled)
                {
                    Current.Explode();
                    Current = null;
                }
                else if (CanUse())
                {
                    Activate();
                    OnUse();
                }
            }
        }

        public override void OnFixedUpdate()
        {
            var human = (Human)_owner;
            if (CanUse())
            {
                if (!human.Setup.ThunderSpearLModel.activeSelf || !human.Setup.ThunderSpearRModel.activeSelf)
                    human.SetThunderSpears(true, true);
            }
            if (Current != null && !Current.Disabled)
            {
                _travelTimeLeft -= Time.fixedDeltaTime;
                if (_travelTimeLeft <= 0f)
                {
                    Current.Explode();
                    Current = null;
                }
            }
        }
    }
}
