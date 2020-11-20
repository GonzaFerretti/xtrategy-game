using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/Transition/Switch Back To Movement")]
public class ControllerTransitionSwitchBackToMovement : ControllerStateTransition
{
    public override bool CheckCondition(BaseController controller)
    {
        return (controller as PlayerController).GetButtonState("changeMode", true) && controller.currentlySelectedUnit.moveState == CurrentActionState.notStarted;
    }

    public override void Transition(BaseController controller)
    {
        base.Transition(controller);
        controller.GetGridReference().EnableCellIndicator(controller.currentlySelectedUnit.GetCoordinates(), GridIndicatorMode.selectedUnit);
        controller.GetGridReference().SetAllCoverIndicators(true);
        if (controller.currentlySelectedUnit.possibleMovements != null) controller.GetGridReference().EnableCellIndicators(controller.currentlySelectedUnit.possibleMovements,GridIndicatorMode.possibleMovement);
        if (controller.currentlySelectedUnit.attackState == CurrentActionState.notStarted)
        {
            controller.GetGridReference().gameManager.hud.EnableHudElementByName("SwitchToAttack");
        }
    }
}
