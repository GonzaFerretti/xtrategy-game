using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AsyncPathQuery : AsyncQuery
{
    public List<Node> finalPath;
    public Vector3Int[] finalPathArray;

    public AsyncPathQuery(int id, GameGridManager gridRef) : base (id,gridRef)
    {
        finalPath = new List<Node>();
        finalPathArray = new Vector3Int[0];
    }

    public Vector3Int[] GetPathArray()
    {
        if (finalPathArray.Length > 0) return finalPathArray;
        return finalPath.Select(n => n.coordinates).ToArray();
    }
}