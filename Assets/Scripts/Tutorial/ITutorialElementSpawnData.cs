using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITutorialElementSpawnData
{
    TutorialElementsSpawnData GetTutorialSpawnData();
}

public struct TutorialElementsSpawnData
{
    public TutorialCoverData coverData;
    public UnitAttributes unitData;
    public ItemData itemData;
    public Vector3Int coordinates;
    public bool isPlayerOwned;
}


