using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ApplicationManagers;
using Settings;
using Characters;
using GameManagers;

namespace UI
{
    class CharacterEditorMenu: BaseMenu
    {
        private CharacterEditorCostumePanel _costumePanel;
        private CharacterEditorStatsPanel _statsPanel;
        private CharacterEditorPreviewPanel _previewPanel;
        public HumanDummy Character;
        public IntSetting Weapon = new IntSetting((int)HumanWeapon.Blade);

        public override void Setup()
        {
            base.Setup();
            Character = ((CharacterEditorGameManager)SceneLoader.CurrentGameManager).Character;
            RebuildPanels();
            ResetCharacter();
        }

        public void RebuildPanels()
        {
            if (_costumePanel != null)
            {
                Destroy(_costumePanel);
                Destroy(_statsPanel);
                Destroy(_previewPanel);
            }
            _costumePanel = ElementFactory.CreateHeadedPanel<CharacterEditorCostumePanel>(transform, enabled: true).GetComponent<CharacterEditorCostumePanel>();
            _statsPanel = ElementFactory.CreateHeadedPanel<CharacterEditorStatsPanel>(transform, enabled: true).GetComponent<CharacterEditorStatsPanel>();
            _previewPanel = ElementFactory.CreateHeadedPanel<CharacterEditorPreviewPanel>(transform, enabled: true).GetComponent<CharacterEditorPreviewPanel>();
            ElementFactory.SetAnchor(_costumePanel.gameObject, TextAnchor.UpperLeft, TextAnchor.UpperLeft, new Vector2(20f, -20f));
            ElementFactory.SetAnchor(_statsPanel.gameObject, TextAnchor.UpperRight, TextAnchor.UpperRight, new Vector2(-20f, -20f));
            ElementFactory.SetAnchor(_previewPanel.gameObject, TextAnchor.UpperRight, TextAnchor.UpperRight, new Vector2(-20f, -450f));
        }

        public void ResetCharacter(bool changeAnimation = false)
        {
            var newSet = (HumanCustomSet)SettingsManager.HumanCustomSettings.CustomSets.GetSelectedSet();
            Character.Setup.Load(newSet, (HumanWeapon)Weapon.Value, false);
            if (changeAnimation)
                Character.Idle();
        }

        public float GetMinMouseX()
        {
            return _costumePanel.GetPhysicalWidth() + 20f;
        }

        public float GetMaxMouseX()
        {
            return Screen.width - _statsPanel.GetPhysicalWidth() - 20f;
        }
    }
}
