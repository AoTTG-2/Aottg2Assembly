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
    class BaseCamera : MonoBehaviour
    {
        public Camera Camera;
        public BaseComponentCache Cache;
        public Skybox Skybox;

        protected virtual void Awake()
        {
            Camera = gameObject.AddComponent<Camera>();
            Skybox = gameObject.AddComponent<Skybox>();
            Cache = new BaseComponentCache(gameObject);
            gameObject.AddComponent<AudioListener>();
            AudioListener.volume = SettingsManager.GeneralSettings.Volume.Value;
            Camera.fieldOfView = 50f;
        }

        public virtual void OnFinishLoading()
        {
            SetDefaultCameraPosition();
        }

        protected virtual void SetDefaultCameraPosition()
        {
        }
    }
}