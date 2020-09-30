using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/Transition/Turn start")]
public class ControllerTransitionTurnStart : ControllerStateTransition
{
    public override bool CheckCondition(BaseController controller)
    {
        return false;
    }
}
