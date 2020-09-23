using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/Transition/Other Actions Available")]
public class ControllerTransitionOtherActionAvailable : ControllerStateTransition
{
    public override bool CheckCondition(BaseController controller)
    {
        return (controller.currentlySelectedUnit.moveState == currentActionState.notStarted && controller.currentlySelectedUnit.attackState == currentActionState.ended)
            || (controller.currentlySelectedUnit.attackState == currentActionState.notStarted && controller.currentlySelectedUnit.moveState == currentActionState.ended);
    }
}
