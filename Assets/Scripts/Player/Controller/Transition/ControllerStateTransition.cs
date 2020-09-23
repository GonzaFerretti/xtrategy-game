using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ControllerStateTransition : ScriptableObject
{
    public ControllerState destinationState;

    public virtual void Transition(BaseController controller)
    {
        controller.SwitchStates(destinationState.stateName);
    }

    public abstract bool CheckCondition(BaseController controller);
}
