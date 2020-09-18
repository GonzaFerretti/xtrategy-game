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
        GameObject unitSelected;
        if ((controller as PlayerController).GetObjectUnderMouse(out unitSelected, 1 << LayerMask.NameToLayer("Unit")))
        {
            controller.currentlySelectedUnit = unitSelected.GetComponent<Unit>();
            controller.currentlySelectedUnit.Select();
        }
    }
}
