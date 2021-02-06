using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/Transition/Item Used")]
public class ControllerTransitionItemUsed : ControllerStateTransition
{
    public override bool CheckCondition(BaseController controller)
    {
        return !(controller as PlayerController).HasItem();
    }
}
