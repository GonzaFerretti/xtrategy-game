using System.Collections;
using System.Collections.Generic;
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
        /*
        if (GUILayout.Button("Test Pathfinding"))
        {
            gridBuilder.TestPathfinding();
        }*/
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
    }

    public void OnSceneGUI()
    {
        if (!gridBuilder) gridBuilder = ((GridBuilder)target);
        if ((gridBuilder.editCoverMode))
        {
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
                    if (canPlaceCover)
                    {
                        gridBuilder.AddCover(possibleCoverPosition, possibleCellMovement);
                    }
                }
            }
        }
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
