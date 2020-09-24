using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Controller/State/Unit selected")]
public class ControllerStateUnitSelected : ControllerState
{

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (Input.GetMouseButtonDown(0)) CheckUnitDeselect();
        if (Input.GetMouseButtonDown(1)) CheckUnitMovement();
    }


    public void CheckUnitDeselect()
    {
        GameObject objectSelected;
        controller.currentlySelectedUnit.Deselect();

        if ((controller as PlayerController).GetObjectUnderMouse(out objectSelected, 1 << LayerMask.NameToLayer("Unit")))
        {
            Unit unitSelected = objectSelected.GetComponent<Unit>();
            if (!controller.OwnsUnit(unitSelected)) return;
            controller.currentlySelectedUnit = unitSelected;
                controller.currentlySelectedUnit.Select();
        }
        else
        {
            controller.currentlySelectedUnit = null;
        }
    }

    void CheckUnitMovement()
    {
        GameObject objectSelected;
        if ((controller as PlayerController).GetObjectUnderMouse(out objectSelected, 1 << LayerMask.NameToLayer("GroundBase")))
        {
            Vector3Int target = objectSelected.transform.parent.GetComponent<GameGridCell>().GetCoordinates();
            controller.MoveUnit(target);
        }
    }
}
