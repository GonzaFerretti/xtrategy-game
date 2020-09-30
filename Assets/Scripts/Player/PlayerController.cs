using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : BaseController
{
    public override void SwitchStates(string identifier)
    {
        //if (lastSelectedCoord) gridManager.DisableCellIndicator(lastSelectedCoord.GetCoordinates());
        lastSelectedCoord = null;
        base.SwitchStates(identifier);
    }

    GameGridCell lastSelectedCoord = null;
    public bool GetObjectUnderMouse(out GameObject hitObject, int layerMaskOfObject)
    {
        hitObject = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 mousePosition = ray.origin;
        RaycastHit hit;
        Debug.DrawLine(ray.origin, ray.origin + ray.direction * 500f, Color.red, 5);
        bool hasHitObject = Physics.Raycast(ray, out hit, 500f, layerMaskOfObject);
        if (hasHitObject)
        {
            hitObject = hit.transform.gameObject;
        }
        return hasHitObject;
    }

    public override void StartTurn()
    {
        base.StartTurn();
        if (GetCurrentStateName() == "waitForTurn")
        {
            currentState.ForceFirstTransition();
        }
    }

    public void OnHoverGrid(GridIndicatorMode possibleCellsMode, GridIndicatorMode selectedCellsMode, List<Vector3Int> listToCheck)
    {
        GameObject objectSelected;
        if (GetObjectUnderMouse(out objectSelected, 1 << LayerMask.NameToLayer("GroundBase")))
        {
            GameGridCell cell = objectSelected.transform.parent.GetComponent<GameGridCell>();
            if (cell == lastSelectedCoord) return;
            Vector3Int coords = cell.GetCoordinates();
            if (listToCheck.Contains(coords))
            {
                if (lastSelectedCoord) gridManager.EnableCellIndicator(lastSelectedCoord.GetCoordinates(), possibleCellsMode);
                gridManager.EnableCellIndicator(coords, selectedCellsMode);
                lastSelectedCoord = cell;
            }
        }
        else
        {
            if (lastSelectedCoord) gridManager.EnableCellIndicator(lastSelectedCoord.GetCoordinates(), possibleCellsMode);
        }
    }
}
