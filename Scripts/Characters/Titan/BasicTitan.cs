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
    class BasicTitan : BaseTitan
    {
        protected BasicTitanComponentCache BasicCache;
        protected BasicTitanAnimations BasicAnimations;
        public override List<string> EmoteActions => new List<string>() { "Salute", "Roar", "Taunt", "Wave" };

        public override void Init(bool ai, string team, JSONNode data)
        {
            base.Init(ai, team, data);
            if (ai)
            {
                var controller = gameObject.AddComponent<BaseTitanAIController>();
                controller.Init(data);
            }
            else
                gameObject.AddComponent<BasicTitanPlayerController>();
        }

        protected override void CreateCache(BaseComponentCache cache)
        {
            BasicCache = new BasicTitanComponentCache(gameObject);
            base.CreateCache(BasicCache);
        }

        protected override void CreateAnimations(BaseTitanAnimations animations)
        {
            BasicAnimations = new BasicTitanAnimations();
            base.CreateAnimations(BasicAnimations);
        }

        public override void Emote(string emote)
        {
            string anim = string.Empty;
            StateAction(TitanState.Emote, anim);
        }

        protected override void Awake()
        {
            base.Awake();
        }

        public override void Attack(string attack)
        {
            _currentAttack = attack;
            string animation = string.Empty;
            Cache.Rigidbody.velocity = Vector3.zero;
            if (_currentAttack == BasicTitanAttacks.AttackCombo1)
            {
            }
            StateAction(TitanState.Attack, animation);
        }

        public virtual void Attack(int combo)
        {
            Attack(BasicTitanAttacks.AttackCombo1);
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
