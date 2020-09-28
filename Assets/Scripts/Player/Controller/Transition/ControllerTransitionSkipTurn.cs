using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/Transition/Skip Turn")]
public class ControllerTransitionSkipTurn : ControllerStateTransition
{
    public override bool CheckCondition(BaseController controller)
    {
        return Input.GetKeyDown(KeyCode.F);
    }

    public override void Transition(BaseController controller)
    {
        base.Transition(controller);
        controller.GetGridReference().gameManager.EndPlayerTurn();
    }
}
