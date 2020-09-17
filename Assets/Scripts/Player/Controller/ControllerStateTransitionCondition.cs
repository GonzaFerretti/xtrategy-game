using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ControllerStateTransitionConditionTransition : ScriptableObject
{
    public abstract void Check(BaseController controller);
}
