using UnityEngine;
using System.Collections;
using Utility;
using System.Collections.Generic;
using Events;
using SimpleJSONFixed;
using Settings;

namespace ApplicationManagers
{
    public class SoundManager : MonoBehaviour
    {
        private static SoundManager _instance;

        public static void Init()
        {
            _instance = SingletonFactory.CreateSingleton(_instance);
            EventManager.OnFinishInit += OnFinishInit;
            EventManager.OnLoadScene += OnLoadScene;
        }

        private static void OnFinishInit()
        {
        }

        private static void OnLoadScene(SceneName sceneName)
        {
        }

        public static void Play(AudioSource source)
        {
        }

        public static void Stop(AudioSource source)
        {

        }
    }
}
