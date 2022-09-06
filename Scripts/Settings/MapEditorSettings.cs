using UnityEngine;
using UI;

namespace Settings
{
    class MapEditorSettings: SaveableSettingsContainer
    {
        protected override string FileName { get { return "MapEditor.json"; } }
        public StringSetting CurrentMap = new StringSetting(string.Empty, maxLength: 100);
    }
}
