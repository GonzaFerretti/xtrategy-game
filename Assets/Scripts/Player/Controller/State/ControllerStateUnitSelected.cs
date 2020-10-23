using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu(menuName = "Controller/State/Unit selected")]
public class ControllerStateUnitSelected : ControllerState
{
    public override void OnUpdate()
    {
        base.OnUpdate();
        if (Input.GetMouseButtonDown(0) && !CheckUnitMovement()) CheckUnitDeselect();
        if (controller.currentlySelectedUnit) (controller as PlayerController).OnHoverGrid(GridIndicatorMode.possibleMovement, GridIndicatorMode.selectedMovement, controller.currentlySelectedUnit.possibleMovements);
    }

    public override void OnTransitionOut()
    {
        controller.GetGridReference().SetAllCoverIndicators(false);
    }

    bool CheckUnitDeselect()
    {
        if ((controller as PlayerController).GetObjectUnderMouse(out GameObject objectSelected, 1 << LayerMask.NameToLayer("Unit")))
        {
            Unit unitSelected = objectSelected.GetComponent<Unit>();
            if (!controller.OwnsUnit(unitSelected)) return false;
            controller.currentlySelectedUnit.Deselect();
            controller.currentlySelectedUnit = unitSelected;
                controller.currentlySelectedUnit.Select();
            return true;
        }
        else
        {
            if (!EventSystem.current.currentSelectedGameObject)
            {
                controller.currentlySelectedUnit.Deselect();
                controller.currentlySelectedUnit = null;
                return true;
            }
            return false;
        }
    }

    bool CheckUnitMovement()
    {
        if ((controller as PlayerController).GetObjectUnderMouse(out GameObject objectSelected, 1 << LayerMask.NameToLayer("GroundBase")))
        {
            Vector3Int target = objectSelected.transform.parent.GetComponent<GameGridCell>().GetCoordinates();
            if (controller.currentlySelectedUnit.possibleMovements.Contains(target))
            {
                controller.currentlySelectedUnit.possibleAttacks = new List<Vector3Int>();
                controller.MoveUnit(target);
                return true;
            }
        }
        return false;
    }
}
