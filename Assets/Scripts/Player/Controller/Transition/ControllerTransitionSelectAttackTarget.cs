using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/Transition/Select Attack")]
public class ControllerTransitionSelectAttackTarget : ControllerStateTransition
{
    public KeyCode requiredKeyCode;
    public override bool CheckCondition(BaseController controller)
    {
        return Input.GetKeyDown(requiredKeyCode) && controller.currentlySelectedUnit.attackState == currentActionState.notStarted;
    }

    public override void Transition(BaseController controller)
    {
        base.Transition(controller);
        controller.currentlySelectedUnit.PrepareAttack();
    }
}