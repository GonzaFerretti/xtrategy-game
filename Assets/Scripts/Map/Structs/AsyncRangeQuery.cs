using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsyncRangeQuery : AsyncQuery
{
    public List<Vector3Int> cellsInRange;
    public List<Vector3Int> lastDepthCells;
    public AsyncRangeQuery(int id, GameGridManager gridRef) : base(id, gridRef)
    {
        cellsInRange = new List<Vector3Int>();
        lastDepthCells = new List<Vector3Int>();
    }
}
