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
using ApplicationManagers;
using Characters;
using System.Linq;
using System.Collections.Generic;

namespace Cameras
{
    class InGameCamera : BaseCamera
    {
        private BaseCharacter _follow;
        private InGameManager _inGameManager;
        private GeneralInputSettings _input;
        public CameraInputMode CurrentCameraMode;
        private float _cameraDistance;
        private float _heightDistance;
        private float _anchorDistance;
        private const float DistanceMultiplier = 10f;


        public void ApplyGraphicsSettings()
        {
            Camera.farClipPlane = SettingsManager.GraphicsSettings.RenderDistance.Value;
        }

        public void ApplyGeneralSettings()
        {
            _cameraDistance = SettingsManager.GeneralSettings.CameraDistance.Value + 0.3f;
        }

        protected override void SetDefaultCameraPosition()
        {
            GameObject go = MapManager.GetRandomTag(MapTags.CameraSpawnPoint);
            if (go != null)
            {
                Cache.Transform.position = go.transform.position;
                Cache.Transform.rotation = go.transform.rotation;
            }
            else
            {
                Cache.Transform.position = Vector3.up * 500f;
                Cache.Transform.rotation = Quaternion.identity;
            }
        }

        public void SetFollow(BaseCharacter character, bool resetRotation = true)
        {
            _follow = character;
            if (_follow == null)
                return;
            if (character is Human)
            {
                _anchorDistance = _heightDistance = 0.64f;
            }
            else if (character is BaseTitan || character is BaseShifter)
            {
                _anchorDistance = Vector3.Distance(character.GetCameraAnchor().position, character.Cache.Transform.position) * 0.4f;
                _heightDistance = Vector3.Distance(character.GetCameraAnchor().position, character.Cache.Transform.position) * 0.45f;
            }
            else
                _anchorDistance = _heightDistance = 1f;
            if (resetRotation)
                Cache.Transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

        protected override void Awake()
        {
            base.Awake();
            ApplyGraphicsSettings();
            ApplyGeneralSettings();
        }

        protected void Start()
        {
            _inGameManager = (InGameManager)SceneLoader.CurrentGameManager;
            _input = SettingsManager.InputSettings.General;
            CurrentCameraMode = (CameraInputMode)SettingsManager.GeneralSettings.CameraMode.Value;
        }
        
        protected void Update()
        {
            if (_follow != _inGameManager.CurrentCharacter && _inGameManager.CurrentCharacter != null)
                SetFollow(_inGameManager.CurrentCharacter);
            if (_follow == null)
                FindNextSpectate();
            if (_follow != null)
            {
                if (_follow == _inGameManager.CurrentCharacter)
                    UpdateMain();
                else
                    UpdateSpectate();
                UpdateObstacles();
            }
            UpdateFOV();
        }

        private void UpdateMain()
        {
            if (_input.ChangeCamera.GetKeyDown())
            {
                if (CurrentCameraMode == CameraInputMode.TPS)
                    CurrentCameraMode = CameraInputMode.Original;
                else if (CurrentCameraMode == CameraInputMode.Original)
                    CurrentCameraMode = CameraInputMode.TPS;
            }
            float offset = _cameraDistance * (200f - Camera.fieldOfView) / 150f;
            Cache.Transform.position = _follow.GetCameraAnchor().position;
            Cache.Transform.position += Vector3.up * _heightDistance;
            Cache.Transform.position -= Vector3.up * (0.6f - _cameraDistance) * 2f;
            float sensitivity = SettingsManager.GeneralSettings.MouseSpeed.Value;
            int invertY = SettingsManager.GeneralSettings.InvertMouse.Value ? -1 : 1;
            if (((InGameMenu)UIManager.CurrentMenu).InMenu())
                sensitivity = 0f;
            if (CurrentCameraMode == CameraInputMode.Original)
            {
                if (Input.mousePosition.x < (Screen.width * 0.4f))
                {
                    float angle = -(((Screen.width * 0.4f) - Input.mousePosition.x) / Screen.width) * 0.4f * 150f * GetSensitivityDeltaTime(sensitivity);
                    Cache.Transform.Rotate(Vector3.up, angle);
                }
                else if (Input.mousePosition.x > (Screen.width * 0.6f))
                {
                    float angle = -((Input.mousePosition.x - (Screen.width * 0.6f)) / Screen.width) * 0.4f * 150f * GetSensitivityDeltaTime(sensitivity);
                    Cache.Transform.Rotate(Vector3.up, angle);
                }
                float rotationX = 0.5f * ((140f * Screen.height * 0.6f) - Input.mousePosition.y) / Screen.height;
                Cache.Transform.rotation = Quaternion.Euler(rotationX, Cache.Transform.rotation.eulerAngles.y, Cache.Transform.rotation.eulerAngles.z);
                Cache.Transform.position -= Cache.Transform.forward * DistanceMultiplier * _anchorDistance * offset;
            }
            else if (CurrentCameraMode == CameraInputMode.TPS)
            {
                float inputX = Input.GetAxis("Mouse X") * 10f * sensitivity;
                float inputY = -Input.GetAxis("Mouse Y") * 10f * sensitivity * invertY;
                Cache.Transform.RotateAround(Cache.Transform.position, Vector3.up, inputX);
                float angleY = Cache.Transform.rotation.eulerAngles.x % 360f;
                float sumY = inputY + angleY;
                bool rotateUp = inputY <= 0f || ((angleY >= 260f || sumY <= 260f) && (angleY >= 80f || sumY <= 80f));
                bool rotateDown = inputY >=0f || ((angleY <= 280f || sumY >= 280f) && (angleY <= 100f || sumY >= 100f));
                if (rotateUp && rotateDown)
                    Cache.Transform.RotateAround(Cache.Transform.position, Cache.Transform.right, inputY);
                Cache.Transform.position -= Cache.Transform.forward * DistanceMultiplier * _anchorDistance * offset;
            }
            if (_cameraDistance < 0.65f)
            {
                // Cache.Transform.position += Cache.Transform.right * Mathf.Max(2f * (0.6f - _cameraDistance), 0.65f);
            }
        }

        private void UpdateSpectate()
        {
            if (_inGameManager.Humans.Count == 0)
                return;
            if (_input.SpectateNextPlayer.GetKeyDown())
            {
                int nextSpectateIndex = GetSpectateIndex() + 1;
                if (nextSpectateIndex >= _inGameManager.Humans.Count)
                    nextSpectateIndex = 0;
                _follow = GetSortedHumans()[nextSpectateIndex];
            }
            if (_input.SpectatePreviousPlayer.GetKeyDown())
            {
                int nextSpectateIndex = GetSpectateIndex() - 1;
                if (nextSpectateIndex < 0)
                    nextSpectateIndex = _inGameManager.Humans.Count - 1;
                _follow = GetSortedHumans()[nextSpectateIndex];
            }
        }

        private void UpdateObstacles()
        {
            Vector3 start = _follow.GetCameraAnchor().position;
            Vector3 direction = (start - Cache.Transform.position).normalized;
            Vector3 end = start - direction * DistanceMultiplier * _anchorDistance;
            LayerMask mask = PhysicsLayer.GetMask(PhysicsLayer.MapObjectMapObjects);
            RaycastHit hit;
            if (Physics.Linecast(start, end, out hit, mask))
                Cache.Transform.position = hit.point;
        }

        private void UpdateFOV()
        {
            if (_follow != null && _follow is Human)
            {
                float speed = _follow.Cache.Rigidbody.velocity.magnitude;
                if (speed > 10f)
                {
                    Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView, Mathf.Min(100f, speed + 40f), 0.1f);
                    return;
                }
                else
                    Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView, 50f, 0.1f);
            }
            else
                Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView, 50f, 0.1f);
        }


        private void FindNextSpectate()
        {
            if (_inGameManager.Humans.Count > 0)
                SetFollow(GetSortedHumans()[0]);
        }

        private int GetSpectateIndex()
        {
            if (_follow == null)
                return -1;
            var humans = GetSortedHumans();
            for (int i = 0; i < humans.Count; i++)
            {
                if (humans[i] == _follow)
                    return i;
            }
            return -1;
        }

        private float GetSensitivityDeltaTime(float sensitivity)
        {
            return (sensitivity * Time.deltaTime) * 62f;
        }

        private List<Human> GetSortedHumans()
        {
            return _inGameManager.Humans.OrderBy(x => x.Cache.PhotonView.ownerId).ToList();
        }
    }
}
