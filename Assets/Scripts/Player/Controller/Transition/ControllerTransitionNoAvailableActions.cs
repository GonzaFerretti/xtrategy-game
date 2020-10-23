using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/Transition/No Available Actions")]
public class ControllerTransitionNoAvailableActions : ControllerStateTransition
{
    public override bool CheckCondition(BaseController controller)
    {
        return (controller.currentlySelectedUnit.moveState == CurrentActionState.ended && controller.currentlySelectedUnit.attackState == CurrentActionState.ended);
    }

    public override void Transition(BaseController controller)
    {
        base.Transition(controller);
    }
}
