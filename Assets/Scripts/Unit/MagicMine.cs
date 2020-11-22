using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicMine : MonoBehaviour
{
    public BaseController owner;
    [HideInInspector] public Vector3Int coordinates;

    public int centerDamage;
    public int sideDamage;

    public int centerDetonateDamage;
    public int sideDetonateDamage;

    public List<Vector3Int> affectedCoordinates = new List<Vector3Int>();
}
