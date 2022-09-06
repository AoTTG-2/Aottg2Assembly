using System;
using UnityEngine;
using ApplicationManagers;
using GameManagers;
using UnityEngine.UI;
using Utility;
using Controllers;
using SimpleJSONFixed;

namespace Characters
{
    class BaseShifter: BaseTitan
    {
        public ShifterComponentCache ShifterCache;
        protected override float DefaultMaxHealth => 2000f;
        protected override float DefaultRunSpeed => 200f;
        protected override float DefaultWalkSpeed => 100f;
        protected override float RotateSpeed => 10f;

        protected override void CreateCache(BaseComponentCache cache)
        {
            ShifterCache = (ShifterComponentCache)cache;
            base.CreateCache(cache);
        }

        public override void Init(bool ai, string team, JSONNode data)
        {
            base.Init(ai, team, data);
            if (ai)
            {
                var controller = gameObject.AddComponent<BaseTitanAIController>();
                controller.Init(data);
            }
            else
                gameObject.AddComponent<ShifterPlayerController>();
        }

        public virtual void Attack(int combo)
        {
        }
    }
}
