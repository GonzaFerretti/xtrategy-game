using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameGridCell : MonoBehaviour
{
    Vector3Int coordinates;
    public void SetCoordinates(Vector3Int coords)
    {
        coordinates = coords;
    }
    public Vector3Int GetCoordinates()
    {
        return coordinates;
    }
}
