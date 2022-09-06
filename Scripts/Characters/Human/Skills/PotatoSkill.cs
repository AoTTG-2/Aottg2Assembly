using System.Collections;
using UnityEngine;

namespace Characters
{
    class PotatoSkill : ExtendedUseable
    {
        protected override float ActiveTime => 10f;
        private const float BuffedSpeed = 30f;
        private float _oldSpeed;

        public PotatoSkill(BaseCharacter owner): base(owner)
        {
        }

        public override bool CanUse()
        {
            return base.CanUse() && ((Human)_owner).Grounded;
        }

        protected override void Activate()
        {
            var human = (Human)_owner;
            _oldSpeed = human.RunSpeed;
            human.RunSpeed = BuffedSpeed;
            human.RunAnimation = HumanAnimations.RunBuffed;
            human.PlayAnimation(HumanAnimations.SpecialSasha);
        }

        protected override void Deactivate()
        {
            var human = (Human)_owner;
            human.RunSpeed = _oldSpeed;
            human.RunAnimation = HumanAnimations.Run;
        }
    }
}
