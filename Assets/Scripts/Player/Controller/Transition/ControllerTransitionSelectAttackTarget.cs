using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/Transition/Select Attack")]
public class ControllerTransitionSelectAttackTarget : ControllerStateTransition
{
    public KeyCode requiredKeyCode;
    public override bool CheckCondition(BaseController controller)
    {
        bool test = Input.GetKeyDown(requiredKeyCode) && controller.currentlySelectedUnit.attackState == currentActionState.notStarted;

        return test;
    }

    public override void Transition(BaseController controller)
    {
        base.Transition(controller);
        controller.currentlySelectedUnit.PrepareAttack();
    }
}