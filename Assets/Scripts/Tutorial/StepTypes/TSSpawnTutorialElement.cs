using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Spawn Element")]
public class TSSpawnTutorialElement : TutorialStep
{
    [SerializeField] ITutorialElementSpawnData spawnDataSource;
    [SerializeField] Vector3Int coordinates;
    [SerializeField] TutorialElementType type;

    public override void OnEnter()
    {
        TutorialElementsSpawnData spawnData = spawnDataSource.GetTutorialSpawnData();

        spawnData.coordinates = coordinates;

        GameObject newElement = tutorialManager.GetGM().SpawnTutorialElement(spawnData, type);

        tutorialManager.spawnedTutorialElements.Add(new SpawnedTutorialElementData { element = newElement, type = type });

        QuickExit();
    }

    public override void OnExit()
    {
    }
}

public enum TutorialElementType
{
    item,
    cover,
    unit,
}

public struct SpawnedTutorialElementData
{
    public GameObject element;
    public TutorialElementType type;
}
