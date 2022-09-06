using System.Collections.Generic;
using UnityEngine;
using UI;
using Utility;
using CustomSkins;
using ApplicationManagers;
using Map;
using Settings;

namespace MapEditor
{
    class PositionGizmo : BaseGizmo
    {
        private Transform _lineX;
        private Transform _lineY;
        private Transform _lineZ;
        private Color SelectedColor = Color.yellow;
        private Color LineXColor = Color.red;
        private Color LineYColor = Color.green;
        private Color LineZColor = Color.blue;
        private Vector3 _totalDelta;
        private Transform _activeLine;
        private Vector3 _previousMousePoint;

        public static PositionGizmo Create()
        {
            var go = AssetBundleManager.InstantiateAsset<GameObject>("PositionGizmo");
            var gizmo = go.AddComponent<PositionGizmo>();
            go.SetActive(false);
            return gizmo;
        }

        protected override void Awake()
        {
            base.Awake();
            _lineX = _transform.Find("LineX");
            _lineY = _transform.Find("LineY");
            _lineZ = _transform.Find("LineZ");
            ResetColors();
        }

        public override void OnSelectionChange()
        {
            if (_gameManager.SelectedObjects.Count > 0)
            {
                gameObject.SetActive(true);
                ResetCenter();
                ResetColors();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        protected void Update()
        {
            var camera = SceneLoader.CurrentCamera;
            float distance = Vector3.Distance(camera.Cache.Transform.position, _transform.position);
            _transform.localScale = Vector3.one * distance / 200f;
            var mouseKey = SettingsManager.InputSettings.MapEditor.Select;
            if (_activeLine == null)
            {
                RaycastHit hit;
                if (!_menu.IsMouseUI && mouseKey.GetKeyDown() && Physics.Raycast(camera.Camera.ScreenPointToRay(Input.mousePosition), out hit, 100000f, PhysicsLayer.GetMask(PhysicsLayer.MapEditorGizmo)))
                {
                    _activeLine = hit.collider.transform;
                    ResetColors();
                    SetLineColor(_activeLine, SelectedColor);
                    _previousMousePoint = hit.point;
                    _totalDelta = Vector3.zero;
                }
            }
            else
            {
                if (mouseKey.GetKey())
                {
                    Ray ray = camera.Camera.ScreenPointToRay(Input.mousePosition);
                    Vector3 mousePoint = ray.origin + ray.direction * Vector3.Distance(ray.origin, _previousMousePoint);
                    Vector3 drag = mousePoint - _previousMousePoint;
                    drag = _activeLine.right * Vector3.Dot(drag, _activeLine.right);
                    Vector3 frameDelta = drag;
                    _totalDelta += frameDelta;
                    MoveSelectedObjects(frameDelta);
                    ResetCenter();
                    _previousMousePoint = mousePoint;
                }
                else
                {
                    _gameManager.NewCommand(new TransformPositionCommand(new List<MapObject>(_gameManager.SelectedObjects)));
                    ResetColors();
                    _activeLine = null;
                    _totalDelta = Vector3.zero;
                }
            }
        }

        private void MoveSelectedObjects(Vector3 frameDelta)
        {
            foreach (MapObject obj in _gameManager.SelectedObjects)
                obj.GameObject.transform.position += frameDelta;
        }

        private void ResetCenter()
        {
            Vector3 totalPosition = new Vector3();
            foreach (MapObject obj in _gameManager.SelectedObjects)
                totalPosition += obj.GameObject.transform.position;
            Vector3 center = totalPosition / _gameManager.SelectedObjects.Count;
            _transform.position = center;
        }

        private void ResetColors()
        {
            SetLineColor(_lineX, LineXColor);
            SetLineColor(_lineY, LineYColor);
            SetLineColor(_lineZ, LineZColor);
        }

        private void SetLineColor(Transform line, Color color)
        {
            foreach (Renderer renderer in line.GetComponentsInChildren<Renderer>())
            {
                renderer.material.color = color;
                renderer.material.renderQueue = 3001;
            }
        }
    }
}
