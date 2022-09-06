using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UnityEngine;
using Settings;
using System.Collections;
using ApplicationManagers;
using GameManagers;
using Characters;
using Map;

namespace UI
{
    class MapEditorTopPanel: HeadedPanel
    {
        protected override float Width => 1960f;
        protected override float Height => 60f;

        protected override float TopBarHeight => 0f;
        protected override float BottomBarHeight => 0f;
        protected override float VerticalSpacing => 0f;
        protected override int HorizontalPadding => 25;
        protected override int VerticalPadding => 0;

        private IntSetting _dropdownSelection = new IntSetting(0);
        private MapEditorMenu _menu;
        private MapEditorGameManager _gameManager;
        private StringSetting _currentMap;
        private List<DropdownSelectElement> _dropdowns = new List<DropdownSelectElement>();
        protected override string ThemePanel => "MapEditor";

        public override void Setup(BasePanel parent = null)
        {
            base.Setup(parent);
            _menu = ((MapEditorMenu)UIManager.CurrentMenu);
            _currentMap = SettingsManager.MapEditorSettings.CurrentMap;
            _gameManager = (MapEditorGameManager)SceneLoader.CurrentGameManager;
            string cat = "MapEditor";
            ElementStyle style = new ElementStyle(titleWidth: 0f, themePanel: ThemePanel);
            float dropdownWidth = 100f;
            Transform group = ElementFactory.CreateHorizontalGroup(SinglePanel, 10f, TextAnchor.MiddleLeft).transform;

            // file dropdown
            List<string> options = new List<string>();
            foreach (string option in new string[] { "New", "Open", "Rename", "Save", "Import", "Export", "LoadPreset", "Quit" })
                options.Add(UIManager.GetLocaleCommon(option));
            var fileDropdown = ElementFactory.CreateDropdownSelect(group, style, _dropdownSelection, UIManager.GetLocale(cat, "Top", "File"),
               options.ToArray(), elementWidth: dropdownWidth, optionsWidth: 140f, maxScrollHeight: 500f, onDropdownOptionSelect: () => OnFileClick());
            _dropdowns.Add(fileDropdown.GetComponent<DropdownSelectElement>());

            // edit dropdown
            options = new List<string>();
            foreach (string option in new string[] {"Undo", "Redo", "Copy", "Paste", "Cut", "Delete"})
            {
                if (option == "Copy" || option == "Delete")
                    options.Add(UIManager.GetLocaleCommon(option));
                else
                    options.Add(UIManager.GetLocale(cat, "Keybinds", option));
            }
            var editDropdown = ElementFactory.CreateDropdownSelect(group, style, _dropdownSelection, UIManager.GetLocaleCommon("Edit"),
               options.ToArray(), elementWidth: dropdownWidth, optionsWidth: 140f, maxScrollHeight: 500f, onDropdownOptionSelect: () => OnEditClick());
            _dropdowns.Add(editDropdown.GetComponent<DropdownSelectElement>());

            // options dropdown
            dropdownWidth = 130f;
            options = new List<string>();
            foreach (string option in new string[] { "MapInfo", "CustomLogic", "CustomAssets", "Editor" })
            {
                options.Add(UIManager.GetLocale(cat, "Top", option));
            }
            var optionsDropdown = ElementFactory.CreateDropdownSelect(group, style, _dropdownSelection, UIManager.GetLocaleCommon("Options"),
               options.ToArray(), elementWidth: dropdownWidth, optionsWidth: 180f, maxScrollHeight: 500f, onDropdownOptionSelect: () => OnOptionsClick());
            _dropdowns.Add(editDropdown.GetComponent<DropdownSelectElement>());

            // gizmos
            ElementFactory.CreateDefaultButton(group, style, UIManager.GetLocale(cat, "Keybinds", "AddObject"), onClick: () => OnButtonClick("AddObject"));
            ElementFactory.CreateDefaultButton(group, style, "Gizmo: Position", onClick: () => OnButtonClick("Gizmo"));
            ElementFactory.CreateDefaultButton(group, style, "Snap: Off", onClick: () => OnButtonClick("Snap"));
            ElementFactory.CreateDefaultButton(group, style, "Camera", onClick: () => OnButtonClick("Camera"));
        }

        public bool IsDropdownOpen()
        {
            foreach (DropdownSelectElement element in _dropdowns)
            {
                if (element.IsOpen())
                    return true;
            }
            return false;
        }

        protected void OnFileClick()
        {
            var files = BuiltinLevels.GetMapNames("Custom").ToList();
            int index = _dropdownSelection.Value;
            var disallowedDelete = new List<string>();
            disallowedDelete.Add(_currentMap.Value);
            if (index == 0) // new
            {
                string newName = "Untitled";
                int i = 1;
                while (files.Contains(newName + i.ToString()))
                    i++;
                _menu.SelectListPopup.ShowSave(files, UIManager.GetLocaleCommon("New"), newName, onSave: () => OnNewFinish(), onDelete: () => OnDeleteMap(),
                    disallowedDelete: disallowedDelete);
            }
            else if (index == 1) // open
            {
                _menu.SelectListPopup.ShowLoad(files, UIManager.GetLocaleCommon("Open"), onLoad: () => OnOpenFinish(), onDelete: () => OnDeleteMap(),
                    disallowedDelete: disallowedDelete);
            }
            else if (index == 2) // rename
            {
                _menu.SelectListPopup.ShowSave(files, UIManager.GetLocaleCommon("Rename"), onSave: () => OnRenameFinish(), onDelete: () => OnDeleteMap(), 
                    disallowedDelete: disallowedDelete);
            }
            else if (index == 3) // save
            {
                var objs = _gameManager.MapScript.Objects.Objects;
                objs.Clear();
                foreach (var obj in MapLoader.IdToMapObject.Values)
                    objs.Add(obj.ScriptObject);
                BuiltinLevels.SaveCustomMap(_currentMap.Value, _gameManager.MapScript);
            }
            else if (index == 4) // import
                _menu.ImportPopup.Show(() => OnImportFinish());
            else if (index == 5) // export
                _menu.ExportPopup.Show(_gameManager.MapScript.Serialize());
            else if (index == 6) // load preset
            {
                var presets = new List<string>();
                foreach (string category in BuiltinLevels.GetMapCategories())
                {
                    if (category == "Custom")
                        continue;
                    foreach (string map in BuiltinLevels.GetMapNames(category))
                        presets.Add(category + "/" + map);
                }
                _menu.SelectListPopup.ShowLoad(presets, UIManager.GetLocaleCommon("LoadPreset"), onLoad: () => OnImportPresetFinish());
            }
            else if (index == 7) // quit
                SceneLoader.LoadScene(SceneName.MainMenu);
        }

        protected void OnEditClick()
        {
            int index = _dropdownSelection.Value;
            if (index == 0) // undo
                _gameManager.Undo();
            else if (index == 1) // redo
                _gameManager.Redo();
            else if (index == 2) // copy
                _gameManager.Copy();
            else if (index == 3) // paste
                _gameManager.Paste();
            else if (index == 4) // cut
                _gameManager.Cut();
            else if (index == 5) // delete
                _gameManager.Delete();
        }

        protected void OnOptionsClick()
        {
            int index = _dropdownSelection.Value;
        }

        protected void OnButtonClick(string name)
        {
            if (name == "AddObject")
            {
            }
        }

        protected void OnDeleteMap()
        {
            BuiltinLevels.DeleteCustomMap(_menu.SelectListPopup.FinishSetting.Value);
        }

        protected void OnNewFinish()
        {
            _currentMap.Value = _menu.SelectListPopup.FinishSetting.Value;
            BuiltinLevels.SaveCustomMap(_currentMap.Value, new MapScript());
            SceneLoader.LoadScene(SceneName.MapEditor);
        }

        protected void OnRenameFinish()
        {
            string oldMap = _currentMap.Value;
            _currentMap.Value = _menu.SelectListPopup.FinishSetting.Value;
            BuiltinLevels.SaveCustomMap(_currentMap.Value, _gameManager.MapScript);
            BuiltinLevels.DeleteCustomMap(oldMap);
        }

        protected void OnOpenFinish()
        {
            _currentMap.Value = _menu.SelectListPopup.FinishSetting.Value;
            SceneLoader.LoadScene(SceneName.MapEditor);
        }

        protected void OnImportFinish()
        {
            MapScript script = new MapScript();
            try
            {
                script.Deserialize(_menu.ImportPopup.ImportSetting.Value);
                BuiltinLevels.SaveCustomMap(_currentMap.Value, script);
                SceneLoader.LoadScene(SceneName.MapEditor);
            }
            catch (Exception e)
            {
                _menu.ImportPopup.ShowError("Error importing: " + e.Message);
            }
        }

        protected void OnImportPresetFinish()
        {
            string[] strArr = _menu.SelectListPopup.FinishSetting.Value.Split('/');
            string category = strArr[0];
            string map = strArr[1];
            MapScript script = new MapScript();
            script.Deserialize(BuiltinLevels.LoadMap(category, map));
            BuiltinLevels.SaveCustomMap(_currentMap.Value, script);
            SceneLoader.LoadScene(SceneName.MapEditor);
        }
    }
}
