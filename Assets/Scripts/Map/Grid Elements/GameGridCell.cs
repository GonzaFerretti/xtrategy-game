using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class GameGridCell : GameGridElement
{
    public void SetCoordinates(Vector3Int coords)
    {
        coordinates = coords;
    }
}
