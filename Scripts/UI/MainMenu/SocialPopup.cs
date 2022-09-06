﻿using ApplicationManagers;
using Settings;
using SimpleJSONFixed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UI
{
    class SocialPopup: BasePopup
    {
        protected override string Title => UIManager.GetLocaleCommon("Social");
        protected override float Width => 630f;
        protected override float Height => 400f;
        protected override bool DoublePanel => false;

        protected override int HorizontalPadding => 35;

        protected override TextAnchor PanelAlignment => TextAnchor.MiddleLeft;


        public override void Setup(BasePanel parent = null)
        {
            base.Setup(parent);
            string cat = "MainMenu";
            string sub = "SocialPopup";
            ElementStyle buttonStyle = new ElementStyle(fontSize: ButtonFontSize, themePanel: ThemePanel);
            ElementFactory.CreateDefaultButton(BottomBar, buttonStyle, UIManager.GetLocaleCommon("Back"), onClick: () => OnButtonClick("Back"));
            ElementStyle mainStyle = new ElementStyle(themePanel: ThemePanel);
            //foreach (JSONNode node in JSON.Parse(AssetBundleManager.LoadText("SocialList")))
            //    CreateLink(mainStyle, node["Title"], node["Link"], node["About"]);
        }

        private void CreateLink(ElementStyle style, string title, string link, string about)
        {
            Transform group = ElementFactory.CreateHorizontalGroup(SinglePanel, 5f).transform;
            ElementFactory.CreateTooltipIcon(group, style, about, 30f, 30f);
            ElementFactory.CreateDefaultLabel(group, style, " " + title + ":");
            ElementFactory.CreateTextButton(group, style, link,
                onClick: () => UIManager.CurrentMenu.ExternalLinkPopup.Show(link));
        }

        private void OnButtonClick(string name)
        {
            if (name == "Back")
                Hide();
        }
    }
}
