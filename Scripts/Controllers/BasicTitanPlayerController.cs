using Settings;
using Characters;
using UnityEngine;

namespace Controllers
{
    class BasicTitanPlayerController: BasePlayerController
    {
        protected BasicTitan _titan;
        protected TitanInputSettings _titanInput;
        protected const float AttackComboTime = 5f;
        protected int _attackComboPhase = 1;
        protected float _attackComboTimeLeft = 0f;

        protected override void Awake()
        {
            base.Awake();
            _titan = GetComponent<BasicTitan>();
            _titanInput = SettingsManager.InputSettings.Titan;
        }

        protected override void Update()
        {
            if (!_inGameMenu.InMenu())
            {
                UpdateMovementInput();
                UpdateActionInput();
            }
            UpdateMenuInput();
            UpdateAttackComboPhase();
        }

        void UpdateActionInput()
        {
            if (_titan.CanAction())
            {
                if (_titanInput.AttackPunch.GetKeyDown())
                {
                    _titan.Attack(_attackComboPhase);
                    NextAttackComboPhase();
                }
                else if (_titanInput.Jump.GetKeyDown())
                    _titan.Jump();
            }
        }

        void UpdateAttackComboPhase()
        {
            _attackComboTimeLeft -= Time.deltaTime;
            if (_attackComboTimeLeft <= 0f)
                _attackComboPhase = 1;
        }

        void NextAttackComboPhase()
        {
            _attackComboPhase++;
            if (_attackComboPhase > 3)
                _attackComboPhase = 1;
        }
    }
}
