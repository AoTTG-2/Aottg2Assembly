using CustomLogic;
using Map;
using Settings;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    class CreateGameModePanel: CreateGameCategoryPanel
    {
        public override void Setup(BasePanel parent = null)
        {
            base.Setup(parent);
            string cat = "CreateGamePopup";
            string sub = "Mode";
            ElementStyle style = new ElementStyle(titleWidth: 200f, themePanel: ThemePanel);
            Dictionary<string, BaseSetting> current = SettingsManager.InGameUI.Mode.Current;
            Dictionary<string, BaseSetting> settings = CustomLogicManager.GetModeSettings(BuiltinLevels.LoadLogic(SettingsManager.InGameUI.General.GameMode.Value));
            foreach (string key in new List<string>(settings.Keys))
            {
                if (current.ContainsKey(key))
                    settings[key] = current[key];
            }
            SettingsManager.InGameUI.Mode.Current = settings;
            ElementFactory.CreateDropdownSetting(DoublePanelLeft, style, SettingsManager.InGameUI.General.GameMode, UIManager.GetLocale(cat, "General", "GameMode"),
                BuiltinLevels.GetGameModes(SettingsManager.InGameUI.General.MapCategory.Value, SettingsManager.InGameUI.General.MapName.Value), 
                elementWidth: 180f, optionsWidth: 240f, onDropdownOptionSelect: () => parent.RebuildCategoryPanel());
            int count = 0;
            foreach (string key in settings.Keys)
            {
                Transform panel = count < settings.Keys.Count / 2 ? DoublePanelLeft : DoublePanelRight;
                BaseSetting setting = settings[key];
                string title = key.Replace("_", " ");
                title = UIManager.GetLocale("CreateGamePopup", sub, title, defaultValue: title);
                if (setting is BoolSetting)
                    ElementFactory.CreateToggleSetting(panel, style, setting, title);
                else if (setting is StringSetting || setting is FloatSetting || setting is IntSetting)
                    ElementFactory.CreateInputSetting(panel, style, setting, title, elementWidth: 180f);
                count += 1;
            }
        }
    }
}
