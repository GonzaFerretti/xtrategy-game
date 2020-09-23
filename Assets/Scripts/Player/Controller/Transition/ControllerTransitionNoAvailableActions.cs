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
}
