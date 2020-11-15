using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct JobNode
{
    public JobNode(Vector3Int coordinates)
    {
        this.coordinates = coordinates;
        parent = new Vector3Int(-1, -1, -1);
        gCost = 0;
        hCost = 0;
    }
    public Vector3Int coordinates;
    public Vector3Int parent;
    public int gCost;
    public int hCost;
    public int GetFCost()
    {
        return gCost + hCost;
    }

    public static int GetDistance(JobNode node1, JobNode node2)
    {
        int result = Mathf.Abs(node1.coordinates.x - node2.coordinates.x) + Mathf.Abs(node1.coordinates.y - node2.coordinates.y);
        return result;
    }
}
