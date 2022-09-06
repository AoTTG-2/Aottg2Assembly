using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UnityEngine;
using Settings;
using System.Collections;

namespace UI
{
    class ScoreboardProfilePopup: PromptPopup
    {
        protected override string Title => UIManager.GetLocaleCommon("Profile");
        protected override float Width => 500f;
        protected override float Height => 500f;

        public override void Setup(BasePanel parent = null)
        {
            base.Setup(parent);
            SetupBottomButtons();
        }

        private void SetupBottomButtons()
        {
            ElementStyle style = new ElementStyle(fontSize: ButtonFontSize, themePanel: ThemePanel);
            foreach (string buttonName in new string[] { "Back" })
            {
                GameObject obj = ElementFactory.CreateDefaultButton(BottomBar, style, UIManager.GetLocaleCommon(buttonName), 
                    onClick: () => OnBottomBarButtonClick(buttonName));
            }
        }

        private void OnBottomBarButtonClick(string name)
        {
            switch (name)
            {
                case "Back":
                    Hide();
                    break;
            }
        }
    }
}
