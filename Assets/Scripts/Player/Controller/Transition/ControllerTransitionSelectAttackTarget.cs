using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/Transition/Select Attack")]
public class ControllerTransitionSelectAttackTarget : ControllerStateTransition
{
    public override bool CheckCondition(BaseController controller)
    {
        bool test = (controller as PlayerController).GetButtonState("changeMode", true) && controller.currentlySelectedUnit.attackState == CurrentActionState.notStarted;

        return test;
    }

    public override void Transition(BaseController controller)
    {
        base.Transition(controller);
        controller.currentlySelectedUnit.PrepareAttack();
        (controller as PlayerController).SetButtonState("changeMode",false);
    }
}