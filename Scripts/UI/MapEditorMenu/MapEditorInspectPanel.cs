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
    class MapEditorInspectPanel: HeadedPanel
    {
        protected override float Width => 400f;
        protected override float Height => 1000f;
        protected override float TopBarHeight => 0f;
        protected override float BottomBarHeight => 0f;
        protected override float VerticalSpacing => 15f;
        protected override int HorizontalPadding => 20;
        protected override int VerticalPadding => 10;
        protected override bool ScrollBar => true;
        private MapEditorMenu _menu;
        private MapObject _mapObject;
        private StringSetting _name = new StringSetting();
        private BoolSetting _active = new BoolSetting();
        private BoolSetting _static = new BoolSetting();
        private BoolSetting _visible = new BoolSetting();
        private FloatSetting _positionX = new FloatSetting();
        private FloatSetting _positionY = new FloatSetting();
        private FloatSetting _positionZ = new FloatSetting();
        private FloatSetting _rotationX = new FloatSetting();
        private FloatSetting _rotationY = new FloatSetting();
        private FloatSetting _rotationZ = new FloatSetting();
        private FloatSetting _scaleX = new FloatSetting();
        private FloatSetting _scaleY = new FloatSetting();
        private FloatSetting _scaleZ = new FloatSetting();
        private StringSetting _collideMode = new StringSetting();
        private StringSetting _collideWith = new StringSetting();
        private StringSetting _physicsMaterial = new StringSetting();
        private StringSetting _shader = new StringSetting();
        private ColorSetting _color = new ColorSetting();
        private StringSetting _texture = new StringSetting();

        public override void Setup(BasePanel parent = null)
        {
            base.Setup(parent);
            _menu = (MapEditorMenu)UIManager.CurrentMenu;
        }

        public void Show(MapObject mapObject)
        {
            base.Show();
            _mapObject = mapObject;
            SyncSettings();
            ElementStyle style = new ElementStyle(fontSize: 22, titleWidth: 60f, spacing: 20f, themePanel: ThemePanel);
            ElementFactory.CreateInputSetting(SinglePanel, style, _name, "Name", elementWidth: 140f, elementHeight: 35f);
            Transform group = ElementFactory.CreateHorizontalGroup(SinglePanel, 20f, TextAnchor.MiddleLeft).transform;
            ElementFactory.CreateToggleSetting(group, style, _active, "Active");
            ElementFactory.CreateToggleSetting(group, style, _static, "Static");
            CreateHorizontalDivider(SinglePanel);
            float inputWidth = 80f;
            style = new ElementStyle(fontSize: 22, titleWidth: 15f, spacing: 5f, themePanel: ThemePanel);
            ElementFactory.CreateDefaultLabel(SinglePanel, style, "Position");
            group = ElementFactory.CreateHorizontalGroup(SinglePanel, 15f, TextAnchor.MiddleLeft).transform;
            ElementFactory.CreateInputSetting(group, style, _positionX, "X", elementWidth: inputWidth, elementHeight: 35f, onEndEdit: () => OnEndEdit());
            ElementFactory.CreateInputSetting(group, style, _positionY, "Y", elementWidth: inputWidth, elementHeight: 35f, onEndEdit: () => OnEndEdit());
            ElementFactory.CreateInputSetting(group, style, _positionZ, "Z", elementWidth: inputWidth, elementHeight: 35f, onEndEdit: () => OnEndEdit());
            ElementFactory.CreateDefaultLabel(SinglePanel, style, "Rotation");
            group = ElementFactory.CreateHorizontalGroup(SinglePanel, 15f, TextAnchor.MiddleLeft).transform;
            ElementFactory.CreateInputSetting(group, style, _rotationX, "X", elementWidth: inputWidth, elementHeight: 35f, onEndEdit: () => OnEndEdit());
            ElementFactory.CreateInputSetting(group, style, _rotationY, "Y", elementWidth: inputWidth, elementHeight: 35f, onEndEdit: () => OnEndEdit());
            ElementFactory.CreateInputSetting(group, style, _rotationZ, "Z", elementWidth: inputWidth, elementHeight: 35f, onEndEdit: () => OnEndEdit());
            ElementFactory.CreateDefaultLabel(SinglePanel, style, "Scale");
            group = ElementFactory.CreateHorizontalGroup(SinglePanel, 15f, TextAnchor.MiddleLeft).transform;
            ElementFactory.CreateInputSetting(group, style, _scaleX, "X", elementWidth: inputWidth, elementHeight: 35f, onEndEdit: () => OnEndEdit());
            ElementFactory.CreateInputSetting(group, style, _scaleY, "Y", elementWidth: inputWidth, elementHeight: 35f, onEndEdit: () => OnEndEdit());
            ElementFactory.CreateInputSetting(group, style, _scaleZ, "Z", elementWidth: inputWidth, elementHeight: 35f, onEndEdit: () => OnEndEdit());
            CreateHorizontalDivider(SinglePanel);
            style = new ElementStyle(fontSize: 22, titleWidth: 160f, spacing: 20f, themePanel: ThemePanel);
            ElementFactory.CreateDropdownSetting(SinglePanel, style, _collideMode, "Collide Mode",
                new string[] { MapObjectCollideMode.Physical, MapObjectCollideMode.Region, MapObjectCollideMode.None },
                elementHeight: 35f);
            ElementFactory.CreateDropdownSetting(SinglePanel, style, _collideWith, "Collide With",
                new string[] { MapObjectCollideWith.Entities, MapObjectCollideWith.Characters, MapObjectCollideWith.Projectiles, 
                    MapObjectCollideWith.MapObjects, MapObjectCollideWith.All}, elementHeight: 35f);
            ElementFactory.CreateDropdownSetting(SinglePanel, style, _physicsMaterial, "Physics Material",
                            new string[] { "Default" }, elementHeight: 35f);
            CreateHorizontalDivider(SinglePanel);
            ElementFactory.CreateToggleSetting(SinglePanel, style, _visible, "Visible");
            ElementFactory.CreateDropdownSetting(SinglePanel, style, _shader, "Shader",
               new string[] { MapObjectShader.Default, MapObjectShader.Basic, MapObjectShader.Transparent, MapObjectShader.Specular },
               elementHeight: 35f);
            ElementFactory.CreateColorSetting(SinglePanel, style, _color, "Color", _menu.ColorPickPopup, onChangeColor: () => OnEndEdit(),
                elementHeight: 25f);
            if (_shader.Value != MapObjectShader.Default)
            {
                group = ElementFactory.CreateHorizontalGroup(SinglePanel, 20f, TextAnchor.MiddleLeft).transform;
                var label = ElementFactory.CreateDefaultLabel(group, style, "Texture", alignment: TextAnchor.MiddleLeft);
                label.GetComponent<LayoutElement>().preferredWidth = 160f;
                ElementFactory.CreateDefaultButton(group, style, _texture.Value, elementWidth: 140f, elementHeight: 35f);
            }
            CreateHorizontalDivider(SinglePanel);
            foreach (var component in ((MapScriptSceneObject)_mapObject.ScriptObject).Components)
            {
                ElementFactory.CreateDefaultLabel(SinglePanel, style, component.ComponentName);
            }
            ElementFactory.CreateDefaultButton(SinglePanel, style, "Add Component", elementWidth: 170f, elementHeight: 40f);
            SinglePanel.gameObject.SetActive(false);
            StartCoroutine(WaitAndEnablePanel());
        }

        private IEnumerator WaitAndEnablePanel()
        {
            yield return new WaitForEndOfFrame();
            SinglePanel.gameObject.SetActive(true);
        }

        private void SyncSettings()
        {
            var script = (MapScriptSceneObject)_mapObject.ScriptObject;
            _name.Value = script.Name;
            _active.Value = script.Active;
            _static.Value = script.Static;
            _visible.Value = script.Visible;
            var position = script.GetPosition();
            _positionX.Value = position.x;
            _positionY.Value = position.y;
            _positionZ.Value = position.z;
            var rotation = script.GetRotation();
            _rotationX.Value = rotation.x;
            _rotationY.Value = rotation.y;
            _rotationZ.Value = rotation.z;
            var scale = script.GetScale();
            _scaleX.Value = scale.x;
            _scaleY.Value = scale.y;
            _scaleZ.Value = scale.z;
            _collideMode.Value = script.CollideMode;
            _collideWith.Value = script.CollideWith;
            _physicsMaterial.Value = script.PhysicsMaterial;
            _shader.Value = script.Material.Shader;
            _color.Value = script.Material.Color;
            if (script.Material is MapScriptBasicMaterial)
            {
                var material = (MapScriptBasicMaterial)script.Material;
                _texture.Value = material.Texture;
            }
        }

        private void OnEndEdit()
        {
           
        }
    }
}
