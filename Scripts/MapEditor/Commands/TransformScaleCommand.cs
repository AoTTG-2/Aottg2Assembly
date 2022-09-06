using System.Collections.Generic;
using UnityEngine;
using UI;
using Utility;
using ApplicationManagers;
using Map;
using GameManagers;

namespace MapEditor
{
    class TransformScaleCommand : BaseCommand
    {
        private List<Vector3> _oldScales;
        private List<Vector3> _newScales;
        private List<int> _ids;

        public TransformScaleCommand(List<MapObject> mapObjects)
        {
            foreach (var mapObject in mapObjects)
            {
                _ids.Add(mapObject.ScriptObject.Id);
                _oldScales.Add(mapObject.ScriptObject.GetScale());
                _newScales.Add(Util.DivideVectors(mapObject.GameObject.transform.localScale, mapObject.BaseScale));
            }
        }

        public override void Execute()
        {
            for (int i = 0; i < _ids.Count; i++)
            {
                var mapObject = MapLoader.IdToMapObject[_ids[i]];
                mapObject.GameObject.transform.localScale = Util.MultiplyVectors(mapObject.BaseScale, _newScales[i]);
                mapObject.ScriptObject.SetScale(_newScales[i]);
            }

        }

        public override void Unexecute()
        {
            for (int i = 0; i < _ids.Count; i++)
            {
                var mapObject = MapLoader.IdToMapObject[_ids[i]];
                mapObject.GameObject.transform.localScale = Util.MultiplyVectors(mapObject.BaseScale, _oldScales[i]);
                mapObject.ScriptObject.SetScale(_oldScales[i]);
            }

        }
    }
}
