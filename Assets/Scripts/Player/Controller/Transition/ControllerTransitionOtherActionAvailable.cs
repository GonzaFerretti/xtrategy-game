using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/Transition/Other Actions Available")]
public class ControllerTransitionOtherActionAvailable : ControllerStateTransition
{
    public override bool CheckCondition(BaseController controller)
    {
        if (!controller.currentlySelectedUnit) return false;
        return (controller.currentlySelectedUnit.moveState == CurrentActionState.notStarted && controller.currentlySelectedUnit.attackState == CurrentActionState.ended)
            || (controller.currentlySelectedUnit.attackState == CurrentActionState.notStarted && controller.currentlySelectedUnit.moveState == CurrentActionState.ended);
    }

    public override void Transition(BaseController controller)
    {
        base.Transition(controller);
        controller.GetGridReference().DisableAllCellIndicators();
        if (controller.currentlySelectedUnit.moveState == CurrentActionState.notStarted)
        {
            controller.GetGridReference().EnableCellIndicators(controller.currentlySelectedUnit.possibleMovements, GridIndicatorMode.possibleMovement);
        }

        if (controller.currentlySelectedUnit.attackState == CurrentActionState.notStarted)
        {
            controller.GetGridReference().gameManager.hud.EnableHudElementByName("SwitchToAttack");
        }

    }
}
