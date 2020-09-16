using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameGridManager), true)]
public class GridEditor : Editor
{
    [SerializeField] Transform lastHoveredCell;
    [SerializeField] Vector3 possibleCoverPosition;
    [SerializeField] CoverData possibleCellMovement;
    [SerializeField] GameGridManager gameGridManager;
    string saveName;

    bool canPlaceCover;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!gameGridManager) gameGridManager = (GameGridManager)target;
        if (GUILayout.Button("New Grid"))
        {
            gameGridManager.InitGrid();
            lastHoveredCell = null;
            canPlaceCover = false;
        }

        if (GUILayout.Button("Clean Grid"))
        {
            gameGridManager.CleanGrid();
            gameGridManager.CleanObstacles();
            lastHoveredCell = null;
            canPlaceCover = false;
        }

        if (GUILayout.Button("Test Pathfinding"))
        {
            gameGridManager.TestPathfinding();
        }
        saveName = EditorGUILayout.TextField(new GUIContent("Save name: "), saveName);

        if (GUILayout.Button("Save to file"))
        {
            string path = "Assets/Scriptable Objects/" + saveName + ".asset";
            MapDictData mapData = AssetDatabase.LoadAssetAtPath<MapDictData>(path);
            if (mapData)
            {
                AssetDatabase.DeleteAsset(path);
            }
            mapData = CreateInstance<MapDictData>();
            mapData.Init(gameGridManager.gridCoordinates, gameGridManager.covers);
            AssetDatabase.CreateAsset(mapData, "Assets/Scriptable Objects/" + saveName + ".asset");
            gameGridManager.SetMapData(mapData);
        }
    }

    public void OnSceneGUI()
    {
        if (!gameGridManager) gameGridManager = (GameGridManager)target;
        if (((GameGridManager)target).editCoverMode)
        {
            if (Event.current.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(0);
            }
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                GameGridManager gameGridManager = (GameGridManager)target;
                Vector3 mousePosition = Event.current.mousePosition;
                Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
                mousePosition = ray.origin;
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100f))
                {
                    if (canPlaceCover)
                    {
                        gameGridManager.AddCover(possibleCoverPosition, possibleCellMovement);
                    }
                }
            }
        }
    }

    void OnEnable()
    {
        if (!gameGridManager) gameGridManager = (GameGridManager)target;
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
                    gameGridManager = (GameGridManager)target;
                    Vector3Int currentlyHoveredCellCoordinates = currentlyHoveredCell.GetComponent<GameGridCell>().GetCoordinates();
                    GameGridCell AdjacentGridCell = gameGridManager.GetAdjacentCellRelativeToMousePosition(hit.point, currentlyHoveredCellCoordinates);
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
