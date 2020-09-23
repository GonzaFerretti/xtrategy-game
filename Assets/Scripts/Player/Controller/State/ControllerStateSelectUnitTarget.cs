using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/State/Select Attack Target")]
public class ControllerStateSelectUnitTarget : ControllerState
{
    public override void OnUpdate()
    {
        base.OnUpdate();
        if (Input.GetMouseButtonDown(1)) CheckPossibleAttackTarget();
    }

    public void CheckPossibleAttackTarget()
    {
        GameObject objectSelected;
        if ((controller as PlayerController).GetObjectUnderMouse(out objectSelected, 1 << LayerMask.NameToLayer("Unit")))
        {
            Unit unitSelected = objectSelected.GetComponent<Unit>();
            if (unitSelected == controller.currentlySelectedUnit) return;
            Vector3Int unitPosition = unitSelected.GetCoordinates();
            if (controller.currentlySelectedUnit.possibleAttacks.Contains(unitPosition))
            {
                Destroy(unitSelected.gameObject);
                controller.currentlySelectedUnit.attackState = currentActionState.ended;
            }
        }
    }
}
