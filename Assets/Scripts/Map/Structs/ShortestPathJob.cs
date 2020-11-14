using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;

public struct ShortestPathJob : IJobParallelFor
{


    public void Execute(int index)
    {
        /*
        Node neighbour = neighbourNodes[index];
        if (closedSet.ContainsKey(neighbour.coordinates))
        {
            continue;
        }

        int newCostToNeighbour = node.gCost + GetDistance(node.coordinates, neighbour.coordinates);
        if (newCostToNeighbour < neighbour.gCost || !openSet.ContainsKey(neighbour.coordinates))
        {
            neighbour.gCost = newCostToNeighbour;
            neighbour.hCost = GetDistance(neighbour.coordinates, targetNode.coordinates);
            neighbour.parent = node;

            if (!openSet.ContainsKey(neighbour.coordinates))
                openSet.Add(neighbour.coordinates, neighbour);
        }*/
    }
}
