using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGridElement : MonoBehaviour
{
    protected GameGridManager grid;

    public void SetGridManagerReference(GameGridManager reference)
    {
        grid = reference;
    }
}
