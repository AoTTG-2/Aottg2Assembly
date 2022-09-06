using Effects;
using UnityEngine;


namespace Characters
{
    class GunWeapon : AmmoWeapon
    {
        public GunWeapon(BaseCharacter owner, int ammo, int ammoPerRound, float cooldown) : base(owner, ammo, ammoPerRound, cooldown)
        {
        }

        protected override void Activate()
        {
            var human = (Human)_owner;
            string anim = "";
            bool left = !human.HookLeft.IsHooked();
            if (human.Grounded)
            {
                if (left)
                    anim = "AHSS_shoot_l";
                else
                    anim = "AHSS_shoot_r";
            }
            else
            {
                if (left)
                    anim = "AHSS_shoot_l_air";
                else
                    anim = "AHSS_shoot_r_air";
            }
            human.State = HumanState.Attack;
            human.AttackAnimation = anim;
            human.CrossFade(anim, 0.05f);
            Vector3 target = human.GetAimPoint();
            Vector3 direction = (target - human.Cache.Transform.position).normalized;
            human.Cache.Transform.rotation = Quaternion.Lerp(human.Cache.Transform.rotation, Quaternion.FromToRotation(human.Cache.Transform.forward, direction),
                Time.deltaTime * 30f);
            human.Cache.Rigidbody.AddForce(human.Cache.Transform.forward * -600f, ForceMode.Acceleration);
            human.Cache.Rigidbody.AddForce(Vector3.up * 200f, ForceMode.Acceleration);
            EffectSpawner.Spawn(EffectPrefabs.GunExplode, human.Cache.Transform.position + human.Cache.Transform.up * 0.8f, human.Cache.Transform.rotation);
            human.StartGunShoot(true);
        }
    }
}
