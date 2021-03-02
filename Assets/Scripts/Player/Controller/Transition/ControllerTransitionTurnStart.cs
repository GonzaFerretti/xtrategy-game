using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/Transition/Turn start")]
public class ControllerTransitionTurnStart : ControllerStateTransition
{
    public override bool CheckCondition(BaseController controller)
    {
        if (!controller || !controller.GetGridReference() || !controller.GetGridReference().gameManager) return false;
        return controller.GetGridReference().gameManager.currentPlayer == controller;
    }
}
