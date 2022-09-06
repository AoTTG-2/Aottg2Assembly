using Settings;
using Characters;
using UnityEngine;

namespace Controllers
{
    class ShifterPlayerController: BasePlayerController
    {
        protected BaseShifter _shifter;
        protected ShifterInputSettings _shifterInput;
        protected const float AttackComboTime = 5f;
        protected int _attackComboPhase = 1;
        protected float _attackComboTimeLeft = 0f;

        protected override void Awake()
        {
            base.Awake();
            _shifter = GetComponent<BaseShifter>();
            _shifterInput = SettingsManager.InputSettings.Shifter;
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
            if (_shifter.CanAction())
            {
                if (_shifterInput.Attack.GetKeyDown())
                {
                    _shifter.Attack(_attackComboPhase);
                    NextAttackComboPhase();
                }
                else if (_shifterInput.Special.GetKeyDown())
                    _shifter.Special();
                else if (_shifterInput.Jump.GetKeyDown())
                    _shifter.Jump();
                else if (_shifterInput.Kick.GetKeyDown())
                    _shifter.Kick();
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
