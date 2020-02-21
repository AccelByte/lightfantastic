using System;
using UnityEngine;
using WebSocketSharp;

public class CharacterParticleLibraryAndResolver: MonoBehaviour
{
    [Serializable]
    public struct CharacterEffectPrefabInfo
    {
        public GameObject prefab;
        public string name;
    }

    [SerializeField] private CharacterEffectPrefabInfo[] prefabInfos;

    GameObject GetPrefab(string name)
    {
        GameObject prefab = new GameObject();
        foreach (var entry in prefabInfos)
        {
            if (entry.name == name)
            {
                prefab = entry.prefab;
            }
        }
        return prefab;
    }
    
    /// <summary>
    /// Select the prefab from asset library and instantiate it immediately
    /// </summary>
    /// <param name="nameOfItem"></param>
    public void Select(string nameOfItem)
    {
        if (nameOfItem.IsNullOrEmpty() || nameOfItem == "NULL")
        {
            return;
        }
        Instantiate(GetPrefab(nameOfItem), transform);
    }

}
