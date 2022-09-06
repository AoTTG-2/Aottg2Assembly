using Map;
using Settings;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;

namespace UI
{
    class CreateGameMiscPanel : CreateGameCategoryPanel
    {
        public override void Setup(BasePanel parent = null)
        {
            base.Setup(parent);
            InGameMiscSettings settings = SettingsManager.InGameUI.Misc;
            string cat = "CreateGamePopup";
            string sub = "Misc";
            ElementStyle style = new ElementStyle(titleWidth: 240f, themePanel: ThemePanel);
            float inputWidth = 120f;
            ElementFactory.CreateToggleGroupSetting(DoublePanelLeft, style, settings.PVP, UIManager.GetLocale(cat, sub, "PVP"), UIManager.GetLocaleArray(cat, sub, "PVPOptions" ));
            ElementFactory.CreateToggleSetting(DoublePanelLeft, style, settings.EndlessRespawnEnabled, UIManager.GetLocale(cat, sub, "EndlessRespawnEnabled"));
            ElementFactory.CreateInputSetting(DoublePanelLeft, style, settings.EndlessRespawnTime, UIManager.GetLocale(cat, sub, "EndlessRespawnTime"), 
                elementWidth: inputWidth);
            ElementFactory.CreateToggleSetting(DoublePanelLeft, style, settings.AllowBlades, UIManager.GetLocale(cat, sub, "AllowBlades"));
            ElementFactory.CreateToggleSetting(DoublePanelLeft, style, settings.AllowGuns, UIManager.GetLocale(cat, sub, "AllowGuns"));
            ElementFactory.CreateToggleSetting(DoublePanelLeft, style, settings.AllowThunderspears, UIManager.GetLocale(cat, sub, "AllowThunderspears"));
            ElementFactory.CreateToggleSetting(DoublePanelLeft, style, settings.AllowPlayerTitans, UIManager.GetLocale(cat, sub, "AllowPlayerTitans"));
            ElementFactory.CreateToggleSetting(DoublePanelLeft, style, settings.AllowShifters, UIManager.GetLocale(cat, sub, "AllowShifters"));
            ElementFactory.CreateToggleSetting(DoublePanelLeft, style, settings.Horses, UIManager.GetLocale(cat, sub, "Horses"));
            ElementFactory.CreateToggleSetting(DoublePanelLeft, style, settings.GunsAirReload, UIManager.GetLocale(cat, sub, "GunsAirReload"));
            ElementFactory.CreateToggleSetting(DoublePanelLeft, style, settings.PreserveKDR, UIManager.GetLocale(cat, sub, "PreserveKDR"));
            ElementFactory.CreateToggleSetting(DoublePanelLeft, style, settings.ClearKDROnRestart, UIManager.GetLocale(cat, sub, "ClearKDROnRestart"));
            ElementFactory.CreateToggleSetting(DoublePanelLeft, style, settings.GlobalMinimapDisable, UIManager.GetLocale(cat, sub, "GlobalMinimapDisable"));
            ElementFactory.CreateInputSetting(DoublePanelLeft, style, settings.Motd, UIManager.GetLocale(cat, sub, "Motd"),
               elementWidth: inputWidth);
        }
    }
}
