using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGridElement : MonoBehaviour
{
    public GameGridManager grid;
    public string basePrefabName;

    public void SetGridManagerReference(GameGridManager reference)
    {
        grid = reference;
    }
}
