using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CellMovement
{
    [SerializeField]
    Vector3Int origin;
    [SerializeField]
    Vector3Int destination;

    public CellMovement(Vector3Int origin, Vector3Int destination)
    {
        this.origin = origin;
        this.destination = destination;
    }
}
