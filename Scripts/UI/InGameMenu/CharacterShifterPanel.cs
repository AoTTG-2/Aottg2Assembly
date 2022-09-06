using GameManagers;
using Map;
using Settings;


namespace UI
{
    class CharacterShifterPanel : CharacterCategoryPanel
    {
        public override void Setup(BasePanel parent = null)
        {
            base.Setup(parent);
            string cat = "CharacterPopup";
            string sub = "General";
            InGameMiscSettings miscSettings = SettingsManager.InGameCurrent.Misc;
            InGameCharacterSettings charSettings = SettingsManager.InGameCharacterSettings;
            charSettings.CharacterType.Value = PlayerCharacter.Shifter;
            ElementStyle dropdownStyle = new ElementStyle(titleWidth: 200f, themePanel: ThemePanel);
            string[] loadouts = new string[] { "Annie", "Eren" };
            ElementFactory.CreateDropdownSetting(DoublePanelLeft, dropdownStyle, charSettings.Loadout, UIManager.GetLocale(cat, sub, "Character"),
                loadouts, elementWidth: 180f, optionsWidth: 180f);
            ElementFactory.CreateDropdownSetting(DoublePanelRight, dropdownStyle, charSettings.Team, UIManager.GetLocaleCommon("Team"),
               new string[] { "Blue", "Red" }, elementWidth: 180f, optionsWidth: 180f);
        }
    }
}

