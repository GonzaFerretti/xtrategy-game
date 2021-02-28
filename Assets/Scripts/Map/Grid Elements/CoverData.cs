using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class CoverData
{
    public Vector3Int side1;
    public Vector3Int side2;
    public CoverData(Vector3Int origin, Vector3Int destination)
    {
        this.side1 = origin;
        this.side2 = destination;
    }

    public static bool operator ==(CoverData lhs, CoverData rhs)
    {
        bool lhsNull = object.ReferenceEquals(lhs, null);
        bool rhsNull = object.ReferenceEquals(rhs, null);
        if (lhsNull && rhsNull)
            return true;
        else if (lhsNull || rhsNull)
            return false;
        return ((lhs.side1 == rhs.side1) && (lhs.side2 == rhs.side2)) || ((lhs.side1 == rhs.side2) && (lhs.side2 == rhs.side1));
    }

    public static bool operator !=(CoverData lhs, CoverData rhs)
    {
        return !(lhs == rhs);
    }

    public bool IsCellMovementDirectionInXAxis()
    {
        return (side1.x == side2.x);
    }
}

public class CoverEqualityComparer : IEqualityComparer<CoverData>
{
    public bool Equals(CoverData c1, CoverData c2)
    {
        return c1 == c2;
    }

    public int GetHashCode(CoverData c)
    {
        return c.side1.GetHashCode() + c.side2.GetHashCode();
    }
}
