using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsyncRangeQuery
{
    public bool hasFinished;
    public List<Vector3Int> cellsInRange;
    public List<Vector3Int> lastDepthCells;
    public GameGridManager grid;
    public int id;
    public AsyncRangeQuery(int id, GameGridManager gridRef)
    {
        hasFinished = false;
        cellsInRange = new List<Vector3Int>();
        lastDepthCells = new List<Vector3Int>();
        grid = gridRef;
        this.id = id;
    }

    public void EndQuery()
    {
        grid.EndQuery(this);
    }
}
