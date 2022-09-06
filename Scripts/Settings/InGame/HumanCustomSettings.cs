using Characters;
using System;
using UnityEngine;

namespace Settings
{
    class HumanCustomSettings : PresetSettingsContainer
    {
        protected override string FileName { get { return "HumanCustom.json"; } }
        public SetSettingsContainer<HumanCustomSet> CustomSets = new SetSettingsContainer<HumanCustomSet>();
        public SetSettingsContainer<HumanCustomSet> Costume1Sets = new SetSettingsContainer<HumanCustomSet>();
        public SetSettingsContainer<HumanCustomSet> Costume2Sets = new SetSettingsContainer<HumanCustomSet>();
        public SetSettingsContainer<HumanCustomSet> Costume3Sets = new SetSettingsContainer<HumanCustomSet>();
    }
}
