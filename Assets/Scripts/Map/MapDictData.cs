using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/MapDictionaries")]
public class MapDictData : ScriptableObject
{
    public GridCoordinates gridCoordinates;
    public CoverInformation coverInfo;

    public void Init(GridCoordinates gridCoordinates, CoverInformation coverInfo)
    {
        this.gridCoordinates = gridCoordinates;
        this.coverInfo = coverInfo;
    }
}
