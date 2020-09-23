using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Node(Vector3Int coordinates) 
    {
        this.coordinates = coordinates;
    }
    public Vector3Int coordinates;
    public Node parent;
    public int gCost;
    public int hCost;
    public int fCost {
        get
        {
            return gCost + hCost;
        }
    }
}
