using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/Transition/Unit Deselect")]
public class ControllerTransitionUnitDeselect : ControllerStateTransition
{
    public override bool CheckCondition(BaseController controller)
    {
        return controller.currentlySelectedUnit == null;
    }
}
