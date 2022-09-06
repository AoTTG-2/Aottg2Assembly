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
using Map;
using System.Collections;
using Utility;

namespace UI
{
    class MapEditorMenu: BaseMenu
    {
        private MapEditorInspectPanel _inspectPanel;
        private MapEditorHierarchyPanel _hierarchyPanel;
        private MapEditorTopPanel _topPanel;
        public bool IsMouseUI;

        public override void Setup()
        {
            base.Setup();
            RebuildPanels();
            _topPanel = ElementFactory.CreateHeadedPanel<MapEditorTopPanel>(transform, true);
            ElementFactory.SetAnchor(_topPanel.gameObject, TextAnchor.UpperCenter, TextAnchor.UpperCenter, new Vector2(0f, 0f));
        }

        public void ShowInspector(MapObject obj)
        {
            HideInspector();
            _inspectPanel = ElementFactory.CreateHeadedPanel<MapEditorInspectPanel>(transform);
            ElementFactory.SetAnchor(_inspectPanel.gameObject, TextAnchor.UpperRight, TextAnchor.UpperRight, new Vector2(-10f, -70f));
            _inspectPanel.Show(obj);
        }

        public void HideInspector()
        {
            if (_inspectPanel != null)
                Destroy(_inspectPanel.gameObject);
        }

        public void ShowHierarchy()
        {
            _hierarchyPanel = ElementFactory.CreateHeadedPanel<MapEditorHierarchyPanel>(transform);
            ElementFactory.SetAnchor(_hierarchyPanel.gameObject, TextAnchor.UpperLeft, TextAnchor.UpperLeft, new Vector2(10f, -70f));
            _hierarchyPanel.Show();
        }

        public bool IsInputFocused()
        {
            return false;
        }

        public void RebuildPanels()
        {
        }

        public float GetMinMouseX()
        {
            if (_hierarchyPanel != null)
                return _hierarchyPanel.GetPhysicalWidth() + 10f;
            return 0f;
        }

        public float GetMaxMouseX()
        {
            float max = Screen.width;
            if (_inspectPanel != null && _inspectPanel.gameObject.activeSelf)
                max -= (_inspectPanel.GetPhysicalWidth() + 10f);
            return max;
        }

        public float GetMinMouseY()
        {
            return 0f;
        }

        public float GetMaxMouseY()
        {
            return Screen.height - 60f;
        }

        private void Update()
        {
            UpdateMouseUI();
        }

        private void UpdateMouseUI()
        {
            var position = Input.mousePosition;
            var pointerEventData = new PointerEventData(EventSystem.current);
            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
            bool mouseUI = raycastResults.Count > 0;
            IsMouseUI = mouseUI || _topPanel.IsDropdownOpen() || position.x < GetMinMouseX() || position.x > GetMaxMouseX() || 
                position.y < GetMinMouseY() || position.y > GetMaxMouseY();
        }
    }
}
