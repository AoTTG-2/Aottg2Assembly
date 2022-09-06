namespace Characters
{
    class BasicTitanAnimations: BaseTitanAnimations
    {
        public override string Idle => "Armature_FemT|ft_idle";
        public override string Run => "Armature_FemT|ft_run";
        public override string Walk => "Armature_FemT|ft_run";
        public override string Jump => "Armature_FemT|ft_idle";
        public override string Fall => "Armature_FemT|ft_idle";
        public override string Land => "Armature_FemT|ft_idle";
        public override string Dodge => "Armature_FemT|ft_dodge";
        public override string Kick => "Armature_FemT|ft_attack_sweep";
        public override string Special => "Armature_FemT|ft_attack_front";
        public override string Block => "Armature_FemT|ft_block";
        public override string Stun => "Armature_FemT|ft_hit_titan";
        public override string Die => "Armature_FemT|ft_die_shifter";
        public override string AttackCombo1 => "Armature_FemT|ft_attack_combo_full";
        public string EmoteSalute = "Armature_FemT|ft_emote_salute";
        public string EmoteTaunt = "Armature_FemT|ft_emote_taunt";
        public string EmoteWave = "Armature_FemT|ft_emote_wave";
        public string EmoteRoar = "Armature_FemT|ft_mad1";
    }
}
