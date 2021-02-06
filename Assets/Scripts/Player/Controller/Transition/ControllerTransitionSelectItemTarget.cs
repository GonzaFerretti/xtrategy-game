using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/Transition/Select Item Target")]
public class ControllerTransitionSelectItemTarget : ControllerStateTransition
{
    public override bool CheckCondition(BaseController controller)
    {
        bool hasPressedItemButton = (controller as PlayerController).GetButtonState("item", true);

        return hasPressedItemButton;
    }

    public override void Transition(BaseController controller)
    {
        base.Transition(controller);
        (controller as PlayerController).SetButtonState("item", false);
    }
}
