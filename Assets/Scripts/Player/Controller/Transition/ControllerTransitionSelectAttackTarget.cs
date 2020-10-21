using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/Transition/Select Attack")]
public class ControllerTransitionSelectAttackTarget : ControllerStateTransition
{
    public KeyCode requiredKeyCode;
    public override bool CheckCondition(BaseController controller)
    {
        bool test = (controller as PlayerController).GetButtonState("changeMode") && controller.currentlySelectedUnit.attackState == currentActionState.notStarted;

        return test;
    }

    public override void Transition(BaseController controller)
    {
        base.Transition(controller);
        controller.currentlySelectedUnit.PrepareAttack();
        (controller as PlayerController).SetButtonState("changeMode",false);
    }
}