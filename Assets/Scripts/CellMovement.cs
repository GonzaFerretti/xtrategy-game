using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CellMovement
{
    public Vector3Int origin;
    public Vector3Int destination;

    public CellMovement(Vector3Int origin, Vector3Int destination)
    {
        this.origin = origin;
        this.destination = destination;
    }

    public bool IsCellMovementDirectionInXAxis()
    {
        return (origin.x == destination.x);
    }
}
