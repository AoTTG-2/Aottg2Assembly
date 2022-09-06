using System.Collections;
using UnityEngine;

namespace Characters
{
    class DistractSkill : ExtendedUseable
    {
        protected override float ActiveTime => 1f;

        public DistractSkill(BaseCharacter owner ): base(owner)
        {
        }

        public override bool CanUse()
        {
            return base.CanUse() && ((Human)_owner).Grounded;
        }

        protected override void Activate()
        {
            var human = (Human)_owner;
            human.PlayAnimation(HumanAnimations.SpecialMarco0);
        }

        protected override void Deactivate()
        {
        }
    }
}
