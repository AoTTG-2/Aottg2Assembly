using UnityEngine;
using CustomLogic;
using Map;
using Characters;
using System.Collections.Generic;

namespace Map
{
    class CustomLogicCollisionHandler: MonoBehaviour
    {
        CustomLogicClassInstance _classInstance;
        CustomLogicEvaluator _evaluator;

        protected void OnCollisionEnter(Collider other)
        {
            var root = other.transform.root;
            if (root.GetComponent<BaseCharacter>() != null)
            {
                
            }
            else
            {
                var mapObject = MapLoader.GoToMapObject[other.gameObject];
            }
        }
    }
}
