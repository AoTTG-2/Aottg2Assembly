using Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UI
{
    class HelpPopup: BasePopup
    {
        protected override string Title => UIManager.GetLocaleCommon("Help");
        protected override float Width => 800f;
        protected override float Height => 510f;
        protected override bool DoublePanel => true;

        public override void Setup(BasePanel parent = null)
        {
            base.Setup(parent);
        }
    }
}
