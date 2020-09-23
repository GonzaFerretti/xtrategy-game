using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/Transition/Start Action")]
public class ControllerTransitionStartAction : ControllerStateTransition
{
    public override bool CheckCondition(BaseController controller)
    {
        return controller.currentlySelectedUnit && (controller.currentlySelectedUnit.moveState == currentActionState.inProgress || controller.currentlySelectedUnit.attackState == currentActionState.inProgress);
    }
}
