using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGridElement : MonoBehaviour
{
    public GameGridManager grid;
    public string basePrefabName;

    [HideInInspector] public Vector3Int coordinates;

    public void SetGridManagerReference(GameGridManager reference)
    {
        grid = reference;
    }

    public virtual Vector3Int GetCoordinates()
    {
        return coordinates;
    }
}
