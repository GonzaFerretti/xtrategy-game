using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridBuilder), true)]
public class GridEditor : Editor
{
    [SerializeField] Transform lastHoveredCell;
    [SerializeField] Vector3 possibleCoverPosition;
    [SerializeField] CoverData possibleCellMovement;
    [SerializeField] GridBuilder gridBuilder;
    string saveName;

    bool canPlaceCover;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!gridBuilder) gridBuilder = ((GridBuilder)target);
        if (GUILayout.Button("New Grid"))
        {
            gridBuilder.InitGrid();
            lastHoveredCell = null;
            canPlaceCover = false;
        }

        if (GUILayout.Button("Clean Grid"))
        {
            gridBuilder.CleanGrid();
            gridBuilder.CleanObstacles();
            lastHoveredCell = null;
            canPlaceCover = false;
        }

        if (saveName == "" && gridBuilder.gameGridManager.savedData) saveName = gridBuilder.gameGridManager.savedData.name;
        saveName = EditorGUILayout.TextField(new GUIContent("Save name: "), saveName);

        if (GUILayout.Button("Save to file"))
        {
            string path = "Assets/Scriptable Objects/Map/" + saveName + ".asset";
            MapDictData mapData = AssetDatabase.LoadAssetAtPath<MapDictData>(path);
            if (mapData)
            {
                AssetDatabase.DeleteAsset(path);
            }
            mapData = CreateInstance<MapDictData>();
            mapData.Init(gridBuilder.gridCoordinates, gridBuilder.covers);
            AssetDatabase.CreateAsset(mapData, path);
            gridBuilder.gameGridManager.savedData = mapData;
        }

        if (GUILayout.Button("Load from file"))
        {
            gridBuilder.CleanGrid();
            gridBuilder.CleanObstacles();

            lastHoveredCell = null;
            canPlaceCover = false;

            string path = "Assets/Scriptable Objects/Map/" + saveName + ".asset";
            MapDictData mapData = AssetDatabase.LoadAssetAtPath<MapDictData>(path);
            gridBuilder.LoadFromFile(mapData);
        }
    }

    public void OnSceneGUI()
    {
        if (!gridBuilder) gridBuilder = ((GridBuilder)target);
        if (Event.current.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(0);
        }
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            Vector3 mousePosition = Event.current.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            mousePosition = ray.origin;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f))
            {
                if (Event.current.shift)
                {
                    if (canPlaceCover)
                    {
                        gridBuilder.AddCover(possibleCoverPosition, possibleCellMovement);
                    }
                }
                else
                {
                    switch (LayerMask.LayerToName(hit.transform.gameObject.layer))
                    {
                        case "GroundBase":
                            GameGridCell nextCell = GetNextPrefab<GameGridCell>("GroundBase", hit);
                            gridBuilder.ReplaceCell(hit.transform.parent.GetComponent<GameGridCell>().GetCoordinates(), nextCell);
                            break;
                        case "LowCover":
                            Cover nextLowCover = GetNextPrefab<LowCover>("LowCover", hit);
                            gridBuilder.ReplaceCover(hit.transform.parent.GetComponent<Cover>().coverData, nextLowCover);
                            break;
                        case "HighCover":
                            Cover nextHighCover = GetNextPrefab<HighCover>("HighCover", hit);
                            gridBuilder.ReplaceCover(hit.transform.parent.GetComponent<Cover>().coverData, nextHighCover);
                            break;

                        default:
                            break;
                    }
                }
            }
        }
    }

    public T GetNextPrefab<T>(string layer, RaycastHit hit)
    {
        GameObject[] possibleFloorPrefabs = gridBuilder.gameGridManager.elementsDatabase.prefabs.Where(n => n.layer == LayerMask.NameToLayer(layer)).ToArray();
        int length = possibleFloorPrefabs.Length;
        int currentIndex = -1;
        GameObject nextPrefab = null;
        for (int i = 0; i < length; i++)
        {
            if (possibleFloorPrefabs[i].name == hit.transform.parent.GetComponent<GameGridElement>().basePrefabName)
            {
                currentIndex = i;
                break;
            }
        }
        if (currentIndex != -1)
        {
            currentIndex++;
            if (currentIndex == length) currentIndex = 0;
            nextPrefab = possibleFloorPrefabs[currentIndex];
        }
        return nextPrefab.GetComponent<T>();
    }

    void OnEnable()
    {
        if (!gridBuilder) gridBuilder = ((GridBuilder)target);
        SceneView.duringSceneGui += this.OnSceneMouseOver;
    }

    void OnSceneMouseOver(SceneView view)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f, 1 << LayerMask.NameToLayer("GroundBase")))
        {
            Transform currentlyHoveredCell = hit.transform.parent;
            if (currentlyHoveredCell == lastHoveredCell)
            {
                if (lastHoveredCell)
                {
                    gridBuilder = ((GridBuilder)target);
                    Vector3Int currentlyHoveredCellCoordinates = currentlyHoveredCell.GetComponent<GameGridCell>().GetCoordinates();
                    GameGridCell AdjacentGridCell = gridBuilder.GetAdjacentCellRelativeToMousePosition(hit.point, currentlyHoveredCellCoordinates);
                    if (AdjacentGridCell)
                    {
                        possibleCoverPosition = (currentlyHoveredCell.position + AdjacentGridCell.transform.position) / 2;
                        possibleCellMovement = new CoverData(AdjacentGridCell.GetCoordinates(), currentlyHoveredCellCoordinates);
                        canPlaceCover = true;
                    }
                    else
                    {
                        canPlaceCover = false;
                    }
                }
            }
            lastHoveredCell = currentlyHoveredCell;
        }
        else
        {
            lastHoveredCell = null;
            canPlaceCover = false;
        }
    }


}
