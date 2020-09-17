using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerStateTransition : ScriptableObject
{
    public ControllerState startingState;
    public ControllerState endState;
    public ControllerStateTransitionConditionTransition condition;
}
