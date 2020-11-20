using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/Transition/Select Attack")]
public class ControllerTransitionSelectAttackTarget : ControllerStateTransition
{
    public override bool CheckCondition(BaseController controller)
    {
        bool hasPressedChangeModeButton = (controller as PlayerController).GetButtonState("changeMode", true);
        bool cannotMove = controller.currentlySelectedUnit.moveState == CurrentActionState.ended;
        bool canAttack = controller.currentlySelectedUnit.attackState == CurrentActionState.notStarted;

        return (hasPressedChangeModeButton || cannotMove) && canAttack;
    }

    public override void Transition(BaseController controller)
    {
        base.Transition(controller);
        controller.currentlySelectedUnit.PrepareAttack();
        (controller as PlayerController).SetButtonState("changeMode", false);
        if (controller.currentlySelectedUnit.moveState == CurrentActionState.notStarted)
        {
            controller.GetGridReference().gameManager.hud.EnableHudElementByName("SwitchToMovement");
        }
    }
}