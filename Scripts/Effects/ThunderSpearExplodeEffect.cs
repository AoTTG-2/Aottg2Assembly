using Photon;
using System;
using UnityEngine;
using System.Collections;
using Settings;

namespace Effects {
    class ThunderSpearExplodeEffect : BaseEffect
    {
        public static float _sizeMultiplier = 1.1f;

        public override void Setup(PhotonPlayer owner, float liveTime, object[] settings)
        {
            base.Setup(owner, liveTime, settings);
            float bombRadius = (float)settings[0];
            float size = Mathf.Clamp(bombRadius, 20f, 60f) * 2f * _sizeMultiplier;
            ParticleSystem particle = GetComponent<ParticleSystem>();
            if (SettingsManager.AbilitySettings.UseOldEffect.Value)
            {
                particle.Stop();
                particle.Clear();
                particle = transform.Find("OldExplodeEffect").GetComponent<ParticleSystem>();
                particle.gameObject.SetActive(true);
                size = size / _sizeMultiplier;
            }
            if (SettingsManager.AbilitySettings.ShowBombColors.Value)
            {
                var c = (Color)settings[1];
                particle.startColor = new Color(c.r, c.g, c.b, Mathf.Max(c.a, 0.5f));
            }
            particle.startSize = size;
        }
    }
}