using UnityEngine;
using Utility;
using Settings;
using UI;
using Weather;
using System.Collections;
using GameProgress;
using Map;
using GameManagers;
using Events;

namespace Cameras
{
    class MapEditorCamera : BaseCamera
    {
        public float MoveSpeed = 250f;
        public float RotateSpeed = 300f;
        private MapEditorInputSettings _input;
        private MapEditorMenu _menu;

        protected override void Awake()
        {
            base.Awake();
            _input = SettingsManager.InputSettings.MapEditor;
        }

        protected override void SetDefaultCameraPosition()
        {
            _menu = (MapEditorMenu)UIManager.CurrentMenu;
        }

        protected void Update()
        {
            if (_menu == null || _menu.IsInputFocused())
                return;
            UpdateMovement();
            if (!_menu.IsMouseUI)
                UpdateRotation();
        }

        private void UpdateMovement()
        {
            Vector3 direction = Vector3.zero;
            if (_input.Forward.GetKey())
                direction += Cache.Transform.forward;
            else if (_input.Back.GetKey())
                direction -= Cache.Transform.forward;
            if (_input.Right.GetKey())
                direction += Cache.Transform.right;
            else if (_input.Left.GetKey())
                direction -= Cache.Transform.right;
            if (_input.Up.GetKey())
                direction += Cache.Transform.up;
            else if (_input.Down.GetKey())
                direction -= Cache.Transform.up;
             Cache.Transform.position += direction * Time.deltaTime * MoveSpeed;
        }

        private void UpdateRotation()
        {
            float mouseX = Input.mousePosition.x;
            if (mouseX < _menu.GetMinMouseX() || mouseX > _menu.GetMaxMouseX())
                return;
            if (_input.Rotate.GetKey())
            {
                float inputX = Input.GetAxis("Mouse X");
                float inputY = Input.GetAxis("Mouse Y");
                Cache.Transform.RotateAround(Cache.Transform.position, Vector3.up, inputX * Time.deltaTime * RotateSpeed);
                Cache.Transform.RotateAround(Cache.Transform.position, Cache.Transform.right, -inputY * Time.deltaTime * RotateSpeed);
            }
        }
    }
}