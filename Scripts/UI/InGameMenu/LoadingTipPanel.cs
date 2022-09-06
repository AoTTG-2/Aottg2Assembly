using ApplicationManagers;
using GameManagers;
using Settings;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    class LoadingTipPanel: BasePopup
    {
        protected override string Title => string.Empty;
        protected override float Width => 100f;
        protected override float Height => 90f;
        protected override float TopBarHeight => 0;
        protected override float BottomBarHeight => 0;
        protected override int VerticalPadding => 20;
        protected override int HorizontalPadding => 20;
        protected override TextAnchor PanelAlignment => TextAnchor.MiddleCenter;

        protected float LabelHeight = 30f;
        protected float _resizedWidth = -1f;
        protected override PopupAnimation PopupAnimationType => PopupAnimation.Fade;
        protected override float AnimationTime => 1f;
        private Text _label;

        public override void Setup(BasePanel parent = null)
        {
            base.Setup(parent);
            ElementStyle defaultStyle = new ElementStyle(themePanel: ThemePanel);
            _label = ElementFactory.CreateDefaultLabel(SinglePanel, defaultStyle, InGameMenu.Tip).GetComponent<Text>();
            _label.GetComponent<LayoutElement>().preferredHeight = LabelHeight;
        }

        protected override float GetWidth()
        {
            if (_resizedWidth == -1f)
            {
                _resizedWidth = ElementFactory.GetTextWidth(UIManager.CurrentMenu.transform, new ElementStyle(), InGameMenu.Tip, FontStyle.Normal);
                _resizedWidth += VerticalPadding * 2f + BorderHorizontalPadding * 2f;
            }
            return _resizedWidth;
        }
    }
}
