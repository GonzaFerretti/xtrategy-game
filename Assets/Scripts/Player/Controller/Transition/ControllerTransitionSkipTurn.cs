using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/Transition/Skip Turn")]
public class ControllerTransitionSkipTurn : ControllerStateTransition
{
    public override bool CheckCondition(BaseController controller)
    {
        return (controller as PlayerController).GetButtonState("skipTurn",true) || !controller.HasAnyMovesLeft();
    }

    public override void Transition(BaseController controller)
    {
        base.Transition(controller);
        (controller as PlayerController).SetButtonState("skipTurn", false);
        controller.GetGridReference().DisableAllCellIndicators(true);
        controller.GetGridReference().gameManager.EndPlayerTurn();
        if (controller.currentlySelectedUnit)
        {
            controller.currentlySelectedUnit.Deselect();
            controller.currentlySelectedUnit = null;
        }
    }
}
