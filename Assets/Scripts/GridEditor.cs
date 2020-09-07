using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameGridManager))]
public class GridEditor : Editor
{
    Transform lastHoveredCell;
    Vector3 possibleCoverPosition;
    CellMovement possibleCellMovement;

    bool canPlaceCover;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GameGridManager gameGridManager = (GameGridManager)target;
        if (GUILayout.Button("New Grid"))
        {
            gameGridManager.InitGrid();
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
        if (Physics.Raycast(ray, out hit, 100f))
        {
            Transform currentlyHoveredCell = hit.transform.parent;
            if (currentlyHoveredCell != lastHoveredCell)
            {
                if (lastHoveredCell)
                {
                    possibleCoverPosition = (currentlyHoveredCell.position + lastHoveredCell.position) / 2;
                    Vector3Int lastHoveredCellCoordinates = lastHoveredCell.GetComponent<GameGridCell>().GetCoordinates();
                    Vector3Int currentlyHoveredCellCoordinates = currentlyHoveredCell.GetComponent<GameGridCell>().GetCoordinates();
                    possibleCellMovement = new CellMovement(lastHoveredCellCoordinates, currentlyHoveredCellCoordinates);
                    canPlaceCover = true;
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
