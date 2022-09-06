using System;
using UnityEngine;
using ApplicationManagers;
using GameManagers;
using UnityEngine.UI;
using Settings;
using Characters;
using UI;

namespace Controllers
{
    class BasePlayerController: MonoBehaviour
    {
        protected GeneralInputSettings _generalInput;
        protected InteractionInputSettings _interactionInput;
        protected InGameMenu _inGameMenu;
        protected BaseCharacter _character;

        protected virtual void Awake()
        {
            _generalInput = SettingsManager.InputSettings.General;
            _interactionInput = SettingsManager.InputSettings.Interaction;
            _character = GetComponent<BaseCharacter>();
            _inGameMenu = (InGameMenu)UIManager.CurrentMenu;
        }

        protected virtual void Start()
        {
        }

        protected virtual void Update()
        {
            if (!_inGameMenu.InMenu())
            {
                UpdateMovementInput();
                UpdateMenuInput();
            }
            UpdateUI();
        }

        protected virtual void UpdateMovementInput()
        {
            int forward = 0;
            int right = 0;
            if (_generalInput.Forward.GetKey())
                forward = 1;
            else if (_generalInput.Back.GetKey())
                forward = -1;
            if (_generalInput.Left.GetKey())
                right = -1;
            else if (_generalInput.Right.GetKey())
                right = 1;
            if (forward != 0 || right != 0)
            {
                _character.TargetAngle = SceneLoader.CurrentCamera.Cache.Transform.rotation.eulerAngles.y + 90f - Mathf.Atan2(forward, right) * Mathf.Rad2Deg;
                _character.HasDirection = true;
            }
            else
                _character.HasDirection = false;
        }

        protected void UpdateMenuInput()
        {
            if (_interactionInput.EmoteMenu.GetKeyDown())
                _inGameMenu.EmoteHandler.ToggleEmoteWheel();
            if (_interactionInput.MenuNext.GetKeyDown())
                _inGameMenu.EmoteHandler.NextEmoteWheel();
        }

        protected virtual void UpdateUI()
        {
        }

        protected float GetTargetAngle(int forward, int right)
        {
            return SceneLoader.CurrentCamera.Cache.Transform.rotation.eulerAngles.y + 90f - Mathf.Atan2(forward, right) * Mathf.Rad2Deg;
        }

        protected Quaternion GetTargetRotation(float angle)
        {
            return Quaternion.Euler(0f, angle, 0f);
        }

        protected Vector3 GetTargetDirection(float angle)
        {
            float angleRadians = (90f - angle) * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(angleRadians), 0f, Mathf.Sin(angleRadians)).normalized;
        }
    }
}
