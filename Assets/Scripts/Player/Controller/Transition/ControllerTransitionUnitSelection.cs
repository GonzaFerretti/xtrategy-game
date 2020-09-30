using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/Transition/Unit Selection")]
public class ControllerTransitionUnitSelection : ControllerStateTransition
{
    public override bool CheckCondition(BaseController controller)
    {
        return controller.currentlySelectedUnit != null && (controller.currentlySelectedUnit.possibleMovements.Count > 0 || controller.currentlySelectedUnit.moveState == currentActionState.ended);
    }

    public override void Transition(BaseController controller)
    {
        base.Transition(controller);
        controller.GetGridReference().EnableCellIndicator(controller.currentlySelectedUnit.GetCoordinates(), GridIndicatorMode.selectedUnit);
        if (controller.currentlySelectedUnit.attackState == currentActionState.notStarted)
        {
            controller.GetGridReference().gameManager.hud.EnableHudElementByName("SwitchToAttack");
        }
    }
}
