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
    class MapEditorHierarchyPanel: HeadedPanel
    {
        protected override float Width => 400f;
        protected override float Height => 1000f;
        protected override float TopBarHeight => 0f;
        protected override float BottomBarHeight => 0f;
        protected override float VerticalSpacing => 15f;
        protected override int HorizontalPadding => 20;
        protected override int VerticalPadding => 10;
        protected override bool ScrollBar => true;
        private List<GameObject> _items = new List<GameObject>();
        private MapEditorMenu _menu;
        private ElementStyle _style;
        

        public override void Setup(BasePanel parent = null)
        {
            base.Setup(parent);
            _menu = (MapEditorMenu)UIManager.CurrentMenu;
            _style = new ElementStyle(fontSize: 22, titleWidth: 0f, themePanel: ThemePanel);
        }

        public void Show()
        {
            base.Show();
            foreach (MapObject obj in MapLoader.IdToMapObject.Values)
            {
                if (obj.Parent < 0)
                    CreateMapItem(obj, 0);
            }
        }

        private void CreateMapItem(MapObject obj, int level)
        {
            string name = obj.ScriptObject.Name;
            for (int i = 0; i < level; i++)
                name = "  " + name;
            var go = ElementFactory.CreateDefaultLabel(SinglePanel, _style, obj.ScriptObject.Name, alignment: TextAnchor.MiddleLeft);
            _items.Add(go);
            if (MapLoader.IdToChildren.ContainsKey(obj.ScriptObject.Id))
            {
                foreach (var child in MapLoader.IdToChildren[obj.ScriptObject.Id])
                    CreateMapItem(MapLoader.IdToMapObject[child], level + 1);
            }
        }


        private void OnEndEdit()
        {
           
        }
    }
}
