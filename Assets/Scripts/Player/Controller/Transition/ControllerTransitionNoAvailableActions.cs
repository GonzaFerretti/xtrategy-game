using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/Transition/No Available Actions")]
public class ControllerTransitionNoAvailableActions : ControllerStateTransition
{
    public override bool CheckCondition(BaseController controller)
    {
        return (controller.currentlySelectedUnit.moveState == currentActionState.ended && controller.currentlySelectedUnit.attackState == currentActionState.ended);
    }

    public override void Transition(BaseController controller)
    {
        base.Transition(controller);
        if (!controller.HasAnyMovesLeft())
        {
            controller.GetGridReference().gameManager.EndPlayerTurn();
            controller.currentlySelectedUnit.Deselect();
            controller.currentlySelectedUnit = null;
        }
    }
}
