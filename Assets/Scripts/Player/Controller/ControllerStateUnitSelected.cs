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
        GameObject unitSelected;
        if (!(controller as PlayerController).GetObjectUnderMouse(out unitSelected, 1 << LayerMask.NameToLayer("Unit")))
        {
            DeselectUnit();
        }
    }

    void CheckUnitMovement()
    {
        GameObject objectSelected;
        if ((controller as PlayerController).GetObjectUnderMouse(out objectSelected, 1 << LayerMask.NameToLayer("GroundBase")))
        {
            Vector3Int target = objectSelected.transform.parent.GetComponent<GameGridCell>().GetCoordinates();
            controller.MoveUnit(target);
            DeselectUnit();
        }
    }

    void DeselectUnit()
    {
        controller.currentlySelectedUnit.Deselect();
        controller.currentlySelectedUnit = null;
    }
}
