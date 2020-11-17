using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{

    public string levelName;
    public UnitSaveInfo[] units;
    public bool hasUsedPower;
    public bool isEnemyTurn;
}

[System.Serializable]
public class UnitSaveInfo
{
    public string owner;
    public int hpLeft;
    public UnitAttributes unitType;
    public bool hasMoved;
    public bool hasAttacked;
    public Vector3Int position;
}
