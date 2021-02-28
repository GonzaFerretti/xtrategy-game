using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Cover Data")]
public class TutorialCoverData : ScriptableObject, ITutorialElementSpawnData
{
    public CoverData coverData;
    public Cover coverPrefab;

    public TutorialElementsSpawnData GetTutorialSpawnData()
    {
        return new TutorialElementsSpawnData() { coverData = this };
    }
}
