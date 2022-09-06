using Map;
using Settings;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;

namespace UI
{
    class CreateGameGeneralPanel: CreateGameCategoryPanel
    {
        public override void Setup(BasePanel parent = null)
        {
            base.Setup(parent);
            InGameSet settings = SettingsManager.InGameUI;
            string cat = "CreateGamePopup";
            string sub = "General";
            ElementStyle dropdownStyle = new ElementStyle(titleWidth: 200f, themePanel: ThemePanel);
            string[] mapNames = BuiltinLevels.GetMapNames(settings.General.MapCategory.Value);
            if (!mapNames.Contains(settings.General.MapName.Value))
                settings.General.MapName.Value = mapNames[0];
            string[] gameModes = BuiltinLevels.GetGameModes(settings.General.MapCategory.Value, settings.General.MapName.Value);
            if (!gameModes.Contains(settings.General.GameMode.Value))
                settings.General.GameMode.Value = gameModes[0];
            ElementFactory.CreateDropdownSetting(DoublePanelLeft, dropdownStyle, settings.General.MapCategory, UIManager.GetLocale(cat, sub, "MapCategory"),
                BuiltinLevels.GetMapCategories(), elementWidth: 180f, optionsWidth: 180f, onDropdownOptionSelect: () => parent.RebuildCategoryPanel());
            ElementFactory.CreateDropdownSetting(DoublePanelLeft, dropdownStyle, settings.General.MapName, UIManager.GetLocale(cat, sub, "MapName"),
                BuiltinLevels.GetMapNames(settings.General.MapCategory.Value), elementWidth: 180f, optionsWidth: 240f, onDropdownOptionSelect: () => parent.RebuildCategoryPanel());
            ElementFactory.CreateDropdownSetting(DoublePanelLeft, dropdownStyle, settings.General.GameMode, UIManager.GetLocale(cat, sub, "GameMode"),
                BuiltinLevels.GetGameModes(settings.General.MapCategory.Value, settings.General.MapName.Value), elementWidth: 180f, optionsWidth: 240f);
            ElementFactory.CreateDropdownSetting(DoublePanelRight, dropdownStyle, settings.WeatherIndex,
                UIManager.GetLocale(cat, sub, "Weather"), SettingsManager.WeatherSettings.WeatherSets.GetSetNames(), elementWidth: 180f);
            ElementFactory.CreateToggleGroupSetting(DoublePanelRight, dropdownStyle, settings.General.Difficulty, UIManager.GetLocale(cat, sub, "Difficulty"),
                UIManager.GetLocaleArray(cat, sub, "DifficultyOptions"));
            if (((CreateGamePopup)parent).IsMultiplayer)
            {
                ElementFactory.CreateInputSetting(DoublePanelLeft, dropdownStyle, settings.General.RoomName, UIManager.GetLocale(cat, sub, "RoomName"), elementWidth: 200f);
                ElementFactory.CreateInputSetting(DoublePanelLeft, dropdownStyle, settings.General.MaxPlayers, UIManager.GetLocale(cat, sub, "MaxPlayers"), elementWidth: 200f);
                ElementFactory.CreateInputSetting(DoublePanelRight, dropdownStyle, settings.General.Password, UIManager.GetLocaleCommon("Password"), elementWidth: 200f);
            }
        }
    }
}
