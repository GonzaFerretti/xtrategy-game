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
    public int GetFCost()
    {
        return gCost + hCost;
    }

    public static int GetDistance(Node node1, Node node2)
    {
        int result = Mathf.Abs(node1.coordinates.x - node2.coordinates.x) + Mathf.Abs(node1.coordinates.y - node2.coordinates.y);
        return result;
    }
}
