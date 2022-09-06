using UnityEngine;
using UI;

namespace Settings
{
    class ProfileSettings: SaveableSettingsContainer
    {
        protected override string FileName { get { return "Profile.json"; } }
        public NameSetting Name = new NameSetting("GUEST" + Random.Range(0, 100000), maxLength: 200, maxStrippedLength: 40);
        public NameSetting Guild = new NameSetting(string.Empty, maxLength: 200, maxStrippedLength: 40);
        public StringSetting Social = new StringSetting(string.Empty, maxLength: 100);
        public StringSetting About = new StringSetting(string.Empty, maxLength: 200);
    }
}
