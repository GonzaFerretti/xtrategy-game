using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Vector3Int coordinates;
    public Node parent;
    public int gCost;
    public int hCost;
}
