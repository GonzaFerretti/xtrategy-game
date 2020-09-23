using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/Transition/Unit Selection")]
public class ControllerTransitionUnitSelection : ControllerStateTransition
{
    public override bool CheckCondition(BaseController controller)
    {
        return controller.currentlySelectedUnit != null && (controller.currentlySelectedUnit.possibleMovements.Count > 0 || controller.currentlySelectedUnit.moveState == currentActionState.ended);
    }
}
