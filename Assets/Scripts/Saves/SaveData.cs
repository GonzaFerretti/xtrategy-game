using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public string levelName;
    public UnitSaveInfo[] units;
    public MineSaveInfo[] mines;
    public PickupItemSaveInfo[] pickupItems;
    public bool hasUsedPower;
    public bool isEnemyTurn;
    public int currentPlayerItemId;
}

[System.Serializable]
public class UnitSaveInfo
{
    public string owner;
    public int hpLeft;
    public int unitId;
    public bool hasMoved;
    public bool hasAttacked;
    public bool isShielded;
    public SavedBuffInfo[] activeBuffs;
    public Vector3Int position;
}

[System.Serializable]
public class MineSaveInfo
{
    public string owner;
    public Vector3Int position;
    public List<Vector3Int> triggerTiles;
    public List<Vector3Int> detonationTiles;
}

[System.Serializable]
public class PickupItemSaveInfo
{
    public Vector3Int position;
    public int itemId;
}

[System.Serializable]
public class SavedBuffInfo
{
    public string identifier;
    public int remainingCharges;
}
