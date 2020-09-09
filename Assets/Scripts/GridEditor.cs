using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameGridManager))]
public class GridEditor : Editor
{
    Transform lastHoveredCell;
    Vector3 possibleCoverPosition;
    CoverData possibleCellMovement;

    bool canPlaceCover;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GameGridManager gameGridManager = (GameGridManager)target;
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
    }

    public void OnSceneGUI()
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

    void OnEnable()
    {
        SceneView.onSceneGUIDelegate += this.OnSceneMouseOver;
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
                    GameGridManager gameGridManager = (GameGridManager)target;
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
