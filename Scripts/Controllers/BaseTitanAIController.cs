using Settings;
using Characters;
using UnityEngine;
using System.Collections.Generic;
using SimpleJSONFixed;

namespace Controllers
{
    class BaseTitanAIController: BaseAIController
    {
        protected BaseTitan _titan;
        public TitanAIState AIState = TitanAIState.Idle;
        public float DetectRange;
        public float AttackRange;
        public float FocusRange;
        public float FocusTime;
        public float ReactionTime;
        public List<string> AttackNames = new List<string>();
        public List<float> AttackChances = new List<float>();
        public float TotalAttackChance;
        protected float _stateTImeLeft;
        protected float _reactionTimeLeft;
        protected float _focusTimeLeft;
        protected BaseCharacter _enemy;
        protected AICharacterDetection _detection;

        protected override void Awake()
        {
            base.Awake();
            _titan = GetComponent<BaseTitan>();
            _detection = AICharacterDetection.Create(_titan, DetectRange);
        }

        public virtual void Init(JSONNode data)
        {
            DetectRange = data["DetectRange"].AsFloat;
            AttackRange = data["AttackRange"].AsFloat;
            FocusRange = data["FocusRange"].AsFloat;
            ReactionTime = data["ReactionTime"].AsFloat;
            foreach (string attack in data["Attacks"].Keys)
            {
                float chance = data["Attacks"][attack];
                AttackNames.Add(attack);
                AttackChances.Add(chance);
                TotalAttackChance += chance;
            }
        }

        protected override void Update()
        {
            _reactionTimeLeft -= Time.deltaTime;
            _focusTimeLeft -= Time.deltaTime;
            if (_reactionTimeLeft > 0f)
                return;
            _reactionTimeLeft = ReactionTime;
            if (_focusTimeLeft <= 0f || _enemy == null)
            {
                _focusTimeLeft = FocusTime;
                _enemy = FindNearestEnemy();
            }
            if (_enemy == null)
            {
                Idle();
            }
            else
            {
                bool inRange = Vector3.Distance(_character.Cache.Transform.position, _enemy.Cache.Transform.position) <= AttackRange;
                if (inRange)
                    MoveToEnemy();
                else
                    AttackEnemy();
            }
        }

        protected void Idle()
        {
            AIState = TitanAIState.Idle;
            _titan.HasDirection = false;
        }

        protected void MoveToEnemy()
        {
            _titan.HasDirection = true;
            _titan.TargetAngle = GetTargetAngle((_enemy.Cache.Transform.position - _character.Cache.Transform.position).normalized);
        }

        protected void AttackEnemy()
        {
            _titan.HasDirection = false;
            if (_titan.CanAction())
                _titan.Attack(GetRandomAttack());
        }

        protected BaseCharacter FindNearestEnemy()
        {
            if (_detection.Enemies.Count == 0)
                return null;
            Vector3 position = _titan.Cache.Transform.position;
            float nearestDistance = float.PositiveInfinity;
            BaseCharacter nearestCharacter = null;
            foreach (BaseCharacter character in _detection.Enemies)
            {
                float distance = Vector3.Distance(character.Cache.Transform.position, position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestCharacter = character;
                }
            }
            return nearestCharacter;
        }

        private string GetRandomAttack()
        {
            float r = Random.Range(0f, TotalAttackChance);
            float start = 0f;
            for (int i = 0; i < AttackNames.Count; i++)
            {
                if (r >= start && r < start + AttackChances[i])
                    return AttackNames[i];
                start += AttackChances[i];
            }
            return AttackNames[0];
        }
    }

    public enum TitanAIState
    {
        Idle,
        Wander,
        MoveToEnemy,
        AttackEnemy
    }
}
