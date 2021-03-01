using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Spawn Element")]
public class TSSpawnTutorialElement : TutorialStep
{
    [Header("Item spawn settings")]
    [SerializeField] ScriptableObject spawnDataSource;
    [SerializeField] string newElementName;
    [Header("Optionals")]
    [SerializeField] Vector3Int coordinates;
    [SerializeField] bool isAllyOwned;
    [SerializeField] bool forceFaceCamera = false;

    public override void OnEnter()
    {
        TutorialElementsSpawnData spawnData = (spawnDataSource as ITutorialElementSpawnData).GetTutorialSpawnData();

        var type = GetSourceType();

        spawnData.coordinates = coordinates;

        spawnData.isPlayerOwned = isAllyOwned;
        spawnData.forceFaceCamera = forceFaceCamera;

        GameObject newElement = tutorialManager.GetGM().SpawnTutorialElement(spawnData, type);

        newElement.name = newElementName;

        tutorialManager.spawnedTutorialElementsData.Add(new SpawnedTutorialElementData { element = newElement, type = type });

        QuickExit();
    }

    TutorialElementType GetSourceType()
    {
        if (spawnDataSource is ItemData) return TutorialElementType.item;
        else if (spawnDataSource is TutorialCoverData) return TutorialElementType.cover;
        else if (spawnDataSource is UnitAttributes) return TutorialElementType.unit;

        Debug.LogError("Wrong spawn item type!");
        return TutorialElementType.error;
    }
}



public enum TutorialElementType
{
    item,
    cover,
    unit,
    error,
}

public struct SpawnedTutorialElementData
{
    public GameObject element;
    public TutorialElementType type;
}
