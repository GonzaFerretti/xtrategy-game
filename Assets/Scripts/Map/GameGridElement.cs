using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGridElement : MonoBehaviour
{
    [SerializeField] public GameGridManager grid;
    [SerializeField] public string basePrefabName;

    public void SetGridManagerReference(GameGridManager reference)
    {
        grid = reference;
    }
}
