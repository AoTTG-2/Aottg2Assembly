using ApplicationManagers;
using GameManagers;
using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
    class CharacterSpawner: MonoBehaviour
    {
        public static BaseCharacter Spawn(string name, Vector3 position, Quaternion rotation)
        {
            GameObject go = PhotonNetwork.Instantiate(name, position, rotation, 0);
            BaseCharacter character = null;
            if (name == CharacterPrefabs.Human)
                character = go.AddComponent<Human>();
            else if (name == CharacterPrefabs.FemaleShifter)
                character = go.AddComponent<FemaleShifter>();
            return character;
        }
    }
}
