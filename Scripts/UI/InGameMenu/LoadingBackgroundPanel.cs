using ApplicationManagers;
using GameManagers;
using Settings;
using SimpleJSONFixed;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    class LoadingBackgroundPanel: BasePopup
    {
        protected override string Title => string.Empty;
        protected override float Width => 0f;
        protected override float Height => 0f;
        protected override float TopBarHeight => 0f;
        protected override float BottomBarHeight => 0f;
        protected GameObject _loadingBackground;
        protected override PopupAnimation PopupAnimationType => PopupAnimation.Fade;
        protected override float AnimationTime => 1f;

        public override void Setup(BasePanel parent = null)
        {
            base.Setup(parent);
            _loadingBackground = ElementFactory.InstantiateAndBind(transform, "LoadingBackground");
            _loadingBackground.AddComponent<IgnoreScaler>();
            var backgrounds = InGameMenu.LoadingBackgroundInfo["Backgrounds"];
            RawImage image = _loadingBackground.GetComponentInChildren<RawImage>();
            image.texture = (Texture2D)AssetBundleManager.LoadAsset(backgrounds[Random.Range(0, backgrounds.Count)].Value);
            float height = (1920f / image.texture.width) * image.texture.height;
            height = Mathf.Max(height, 1080f);
            image.GetComponent<RectTransform>().sizeDelta = new Vector2(1920f, height);
        }
    }
}
