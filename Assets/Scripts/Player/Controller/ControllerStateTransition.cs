using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ControllerStateTransition : ScriptableObject
{
    public ControllerState destinationState;
    public abstract void Transition(BaseController controller);
    public abstract bool CheckCondition(BaseController controller);
}
