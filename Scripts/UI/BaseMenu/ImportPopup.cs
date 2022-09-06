﻿using Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Weather;

namespace UI
{
    class ImportPopup: PromptPopup
    {
        protected override string Title => UIManager.GetLocaleCommon("Import");
        protected override float Width => 500f;
        protected override float Height => 600f;
        protected override int VerticalPadding => 20;
        protected override int HorizontalPadding => 20;
        protected override float VerticalSpacing => 10f;
        private UnityAction _onSave;
        private InputSettingElement _element;
        private Text _text;
        public StringSetting ImportSetting = new StringSetting(string.Empty);
        private bool _manualHide;

        public override void Setup(BasePanel parent = null)
        {
            base.Setup(parent);
            ElementStyle style = new ElementStyle(fontSize: ButtonFontSize, themePanel: ThemePanel);
            ElementFactory.CreateDefaultButton(BottomBar, style, UIManager.GetLocaleCommon("Save"), onClick: () => OnButtonClick("Save"));
            ElementFactory.CreateDefaultButton(BottomBar, style, UIManager.GetLocaleCommon("Cancel"), onClick: () => OnButtonClick("Cancel"));
            _element = ElementFactory.CreateInputSetting(SinglePanel, style, ImportSetting, string.Empty, elementWidth: 460f, elementHeight: 390f, multiLine: true).
                GetComponent<InputSettingElement>();
            _text = ElementFactory.CreateDefaultLabel(SinglePanel, style, "").GetComponent<Text>();
            _text.color = Color.red;
        }

        public void Show(UnityAction onSave, bool manualHide = true)
        {
            if (gameObject.activeSelf)
                return;
            base.Show();
            _onSave = onSave;
            _manualHide = manualHide;
            ImportSetting.Value = string.Empty;
            _text.text = string.Empty;
            _element.SyncElement();
        }

        public void ShowError(string error)
        {
            _text.text = error;
        }

        private void OnButtonClick(string name)
        {
            if (name == "Cancel")
            {
                Hide();
            }
            else if (name == "Save")
            {
                _onSave.Invoke();
                if (!_manualHide)
                    Hide();
            }
        }
    }
}
