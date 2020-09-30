using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/Transition/Skip Turn")]
public class ControllerTransitionSkipTurn : ControllerStateTransition
{
    public override bool CheckCondition(BaseController controller)
    {
        return Input.GetKeyDown(KeyCode.F) || !controller.HasAnyMovesLeft();
    }

    public override void Transition(BaseController controller)
    {
        base.Transition(controller);
        controller.GetGridReference().DisableAllCellIndicators();
        controller.GetGridReference().gameManager.EndPlayerTurn();
        controller.currentlySelectedUnit.Deselect();
        controller.currentlySelectedUnit = null;
    }
}
