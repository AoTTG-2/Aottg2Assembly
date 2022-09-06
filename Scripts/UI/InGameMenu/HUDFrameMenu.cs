using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Settings;
using GameManagers;
using Utility;

namespace UI
{
    class HUDFrameMenu: BaseMenu
    {

        public override void Setup()
        {
            base.Setup();
            gameObject.AddComponent<CrosshairHandler>();
        }

        protected override void SetupPopups()
        {
        }
    }
}
