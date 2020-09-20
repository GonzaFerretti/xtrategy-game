using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/Map Elements Database")]
public class MapElementsDB : ScriptableObject
{
    public GameObject[] prefabs;

    public T GetElementByType<T>(string name)
    {
        T valueToReturn = default(T);
        foreach (GameObject go in prefabs)
        {
            if (go.name == name)
            {
                go.TryGetComponent<T>(out valueToReturn);
                break;
            }
        }
        return valueToReturn;
    }

    public GameObject GetElement(string name)
    {
        GameObject objectToReturn = null;
        foreach (GameObject go in prefabs)
        {
            if (go.name == name)
            {
                objectToReturn = go;
                break;
            }
        }
        return objectToReturn;
    }
}
