using System.Collections;
using UnityEngine;

namespace Characters
{
    class EscapeSkill : ExtendedUseable
    {
        protected override float ActiveTime => 0.64f;

        public EscapeSkill(BaseCharacter owner ): base(owner)
        {
        }

        public override bool CanUse()
        {
            return base.CanUse() && ((Human)_owner).State == HumanState.Grab;
        }

        protected override void Activate()
        {
            ((Human)_owner).PlayAnimation("grabbed_jean");
        }

        protected override void Deactivate()
        {
        }
    }
}
