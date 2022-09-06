namespace Characters
{
    class BaseTitanAnimations
    {
        public virtual string Idle => "idle";
        public virtual string Run => "run";
        public virtual string Walk => "walk";
        public virtual string Jump => "jumpCombo_1";
        public virtual string Fall => "front";
        public virtual string Land => "front";
        public virtual string Dodge => "front";
        public virtual string Kick => "sweep";
        public virtual string Special => "combo_1";
        public virtual string Block => "";
        public virtual string Stun => "";
        public virtual string Die => "";
        public virtual string AttackCombo1 => "";
        public virtual string AttackCombo2 => "";
        public virtual string AttackCombo3 => "";
    }
}
