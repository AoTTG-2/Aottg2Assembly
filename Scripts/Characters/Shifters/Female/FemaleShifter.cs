using System;
using UnityEngine;
using ApplicationManagers;
using GameManagers;
using UnityEngine.UI;
using Utility;
using Controllers;
using CustomSkins;
using System.Collections.Generic;
using SimpleJSONFixed;

namespace Characters
{
    class FemaleShifter: BaseShifter
    {
        protected FemaleComponentCache FemaleCache;
        protected FemaleAnimations FemaleAnimations;
        protected AnnieCustomSkinLoader _customSkinLoader;
        public override List<string> EmoteActions => new List<string>() { "Salute", "Roar", "Taunt", "Wave" };
        protected int _stepSoundPhase = 0;

        protected override void CreateCache(BaseComponentCache cache)
        {
            FemaleCache = new FemaleComponentCache(gameObject);
            base.CreateCache(FemaleCache);
        }

        protected override void CreateAnimations(BaseTitanAnimations animations)
        {
            FemaleAnimations = new FemaleAnimations();
            base.CreateAnimations(FemaleAnimations);
        }

        public override void Emote(string emote)
        {
            string anim = string.Empty;
            if (emote == "Salute")
                anim = FemaleAnimations.EmoteSalute;
            else if (emote == "Roar")
                anim = FemaleAnimations.EmoteRoar;
            else if (emote == "Taunt")
                anim = FemaleAnimations.EmoteTaunt;
            else if (emote == "Wave")
                anim = FemaleAnimations.EmoteWave;
            StateAction(TitanState.Emote, anim);
        }

        protected override void Awake()
        {
            base.Awake();
            _customSkinLoader = gameObject.AddComponent<AnnieCustomSkinLoader>();
        }

        public override void Attack(string attack)
        {
            _currentAttack = attack;
            string animation = string.Empty;
            Cache.Rigidbody.velocity = Vector3.zero;
            if (_currentAttack == FemaleAttacks.AttackCombo1)
            {
                animation = FemaleAnimations.AttackCombo1;
                FemaleCache.ShinLHitbox.Activate(0.1f, 2f);
            }
            StateAction(TitanState.Attack, animation);
        }

        public override void Attack(int combo)
        {
            Attack(FemaleAttacks.AttackCombo1);
        }

        public override void Special()
        {
            base.Special();
        }

        public override void OnHit(BaseHitbox hitbox, BaseCharacter character, Collider collider)
        {
        }
    }
}
