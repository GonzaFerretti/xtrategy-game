using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization;

[CreateAssetMenu(menuName = "Map/MapDictionaries")]
public class MapDictData : ScriptableObject
{
    [Header("Covers")]
    public List<CoverData> coversData;
    public List<string> coversPrefabNames;

    [Header("Cells")]
    public List<Vector3Int> cellsCoordinates;
    public List<string> cellsPrefabNames;
    public bool hasBoss;

    [Header("Items")]
    public List<ItemData> itemsToSpawn;

    public void Init(GridCoordinates gridCoordinates, CoverInformation coverInfo)
    {
        cellsCoordinates = new List<Vector3Int>();
        cellsPrefabNames = new List<string>();

        foreach (KeyValuePair<Vector3Int, GameGridCell> cellData in gridCoordinates)
        {
            cellsCoordinates.Add(cellData.Key);
            cellsPrefabNames.Add(cellData.Value.basePrefabName);
        }

        coversData = new List<CoverData>();
        coversPrefabNames = new List<string>();

        foreach (KeyValuePair<CoverData, Cover> coverData in coverInfo)
        {
            coversData.Add(coverData.Key);
            coversPrefabNames.Add(coverData.Value.basePrefabName);
        }
    }
}

[Serializable]
public class SerializedCellData : Dictionary<Vector3Int, string> 
{
    public SerializedCellData() : base() { }
    public SerializedCellData(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }
}

[Serializable]
public class SerializedCoverData : Dictionary<CoverData, string> 
{
    public SerializedCoverData() : base() { }
    public SerializedCoverData(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }
}
