using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsyncQuery
{
    public bool hasFinished;
    public GameGridManager grid;
    public int id;
    public AsyncQuery(int id, GameGridManager gridRef)
    {
        hasFinished = false;
        grid = gridRef;
        this.id = id;
    }

    public void End()
    {
        grid.EndQuery(this);
    }
}
