using Settings;
using UnityEngine.UI;

namespace UI
{
    class EditProfileProfilePanel: CategoryPanel
    {
        public override void Setup(BasePanel parent = null)
        {
            base.Setup(parent);
            ProfileSettings settings = SettingsManager.ProfileSettings;
            ElementStyle style = new ElementStyle(titleWidth: 120f, themePanel: ThemePanel);
            ElementFactory.CreateInputSetting(SinglePanel, style, settings.Name, UIManager.GetLocaleCommon("Name"), elementWidth: 250f);
            ElementFactory.CreateInputSetting(SinglePanel, style, settings.Guild, UIManager.GetLocaleCommon("Guild"), elementWidth: 250f);
            ElementFactory.CreateInputSetting(SinglePanel, style, settings.Social, UIManager.GetLocaleCommon("Social"), elementWidth: 250f);
            ElementFactory.CreateInputSetting(SinglePanel, style, settings.About, UIManager.GetLocaleCommon("About"), elementWidth: 250f, elementHeight: 120f, 
                multiLine: true);
        }
    }
}
