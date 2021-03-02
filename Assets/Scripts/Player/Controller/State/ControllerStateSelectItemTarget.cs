using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/State/Select Item Target")]
public class ControllerStateSelectItemTarget : ControllerState
{
    public override void OnUpdate()
    {
        base.OnUpdate();
        if (Input.GetMouseButtonDown(0) && !CheckItemUse()) (controller as PlayerController).SwitchStates("unitDeselected");
    }

    public override void OnTransitionIn()
    {
        base.OnTransitionIn();
        foreach (var unit in controller.unitsControlled)
        {
            controller.GetGridReference().EnableCellIndicator(unit.GetCoordinates(), GridIndicatorMode.attackRange);
        }
    }

    bool CheckItemUse()
    {
        if ((controller as PlayerController).GetObjectUnderMouse(out Unit unitSelected, 1 << LayerMask.NameToLayer("Unit")))
        {
            if (!controller.OwnsUnit(unitSelected)) return false;
            (controller as PlayerController).UseItem(unitSelected);
            return true;
        }
        return false;
    }

    public override void OnTransitionOut()
    {
        controller.GetGridReference().DisableAllCellIndicators();
        controller.GetGridReference().gameManager.hud.EnableHudElementByName("ItemButton");
    }
}

