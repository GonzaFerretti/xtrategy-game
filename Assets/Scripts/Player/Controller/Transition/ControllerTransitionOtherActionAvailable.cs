using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/Transition/Other Actions Available")]
public class ControllerTransitionOtherActionAvailable : ControllerStateTransition
{
    public override bool CheckCondition(BaseController controller)
    {
        if (!controller.currentlySelectedUnit) return false;
        return (controller.currentlySelectedUnit.moveState == currentActionState.notStarted && controller.currentlySelectedUnit.attackState == currentActionState.ended)
            || (controller.currentlySelectedUnit.attackState == currentActionState.notStarted && controller.currentlySelectedUnit.moveState == currentActionState.ended);
    }

    public override void Transition(BaseController controller)
    {
        base.Transition(controller);
        if (controller.currentlySelectedUnit.moveState == currentActionState.notStarted)
        {
            controller.GetGridReference().TintBulk(controller.currentlySelectedUnit.possibleMovements);
        }
    }
}
