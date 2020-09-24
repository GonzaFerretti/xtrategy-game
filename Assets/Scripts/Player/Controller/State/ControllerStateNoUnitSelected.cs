using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/State/No unit selected")]
public class ControllerStateNoUnitSelected : ControllerState
{
    public override void OnUpdate()
    {
        base.OnUpdate();
        if (Input.GetMouseButtonDown(0)) CheckUnitSelect();
    }

    public void CheckUnitSelect()
    {
        GameObject objectSelected;
        if ((controller as PlayerController).GetObjectUnderMouse(out objectSelected, 1 << LayerMask.NameToLayer("Unit")))
        {
            Unit unitSelected = objectSelected.GetComponent<Unit>();
            if (!controller.OwnsUnit(unitSelected)) return;
            controller.currentlySelectedUnit = unitSelected;
            controller.currentlySelectedUnit.Select();
        }
    }
}
