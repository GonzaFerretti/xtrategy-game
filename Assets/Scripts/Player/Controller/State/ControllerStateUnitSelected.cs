using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/State/Unit selected")]
public class ControllerStateUnitSelected : ControllerState
{
    public override void OnUpdate()
    {
        base.OnUpdate();
        if (Input.GetMouseButtonDown(0) && !CheckUnitMovement()) (controller as PlayerController).CheckUnitDeselect();
        if (controller.currentlySelectedUnit) (controller as PlayerController).OnHoverGrid(GridIndicatorMode.movementRange, GridIndicatorMode.selectedMovement, controller.currentlySelectedUnit.possibleMovements);
    }

    public override void OnTransitionOut()
    {
        controller.GetGridReference().SetAllCoverIndicators(false);
        controller.GetGridReference().DisableAllCellIndicators(true);
    }

    public override void OnTransitionIn()
    {
        base.OnTransitionIn();
        controller.GetGridReference().EnableCellIndicator(controller.currentlySelectedUnit.GetCoordinates(), GridIndicatorMode.selectedUnit);
    }

    bool CheckUnitMovement()
    {
        (controller as PlayerController).CheckUnitUISwitch();
        if ((controller as PlayerController).GetObjectUnderMouse(out GameObject objectSelected, 1 << LayerMask.NameToLayer("GroundBase")))
        {
            Vector3Int target = objectSelected.transform.parent.GetComponent<GameGridCell>().GetCoordinates();
            if (!controller.currentlySelectedUnit) return false;
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
