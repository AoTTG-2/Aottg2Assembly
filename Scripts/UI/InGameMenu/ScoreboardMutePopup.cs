﻿using Settings;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    class ScoreboardMutePopup: PromptPopup
    {
        protected override string Title => UIManager.GetLocale("ScoreboardPopup", "MutePopup", "Title");
        protected override int VerticalPadding => 20;
        protected override int HorizontalPadding => 20;
        protected override float VerticalSpacing => 20f;
        protected override float Width => 370f;
        protected override float Height => 250f;
        protected override TextAnchor PanelAlignment => TextAnchor.MiddleCenter;

        public override void Setup(BasePanel parent = null)
        {
            base.Setup(parent);
            string cat = "ScoreboardPopup";
            string sub = "MutePopup";
            ElementStyle buttonStyle = new ElementStyle(fontSize: ButtonFontSize, themePanel: ThemePanel);
            ElementStyle style = new ElementStyle(titleWidth: 240f, themePanel: ThemePanel);
            ElementFactory.CreateDefaultButton(BottomBar, buttonStyle, UIManager.GetLocaleCommon("Confirm"), onClick: () => OnButtonClick("Confirm"));
            //ElementFactory.CreateToggleSetting(SinglePanel, style, roomListPopup._filterShowFull, UIManager.GetLocale(cat, sub, "ShowFull"));
            //ElementFactory.CreateToggleSetting(SinglePanel, style, roomListPopup._filterShowPassword, UIManager.GetLocale(cat, sub, "ShowPassword"));
        }
        protected void OnButtonClick(string name)
        {
            if (name == "Confirm")
            {
                Hide();
            }
        }
    }
}
