using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct CoverData
{
    Vector3Int side1;
    Vector3Int side2;
    public CoverData(Vector3Int origin, Vector3Int destination)
    {
        this.side1 = origin;
        this.side2 = destination;
    }

    public CoverData GetInverted()
    {
        return new CoverData(this.side2, this.side1);
    }

    public bool IsCellMovementDirectionInXAxis()
    {
        return (side1.x == side2.x);
    }
}
