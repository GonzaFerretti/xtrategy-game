using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ControllerStateTransition : ScriptableObject
{
    public ControllerState destinationState;
    public List<string> hudElementsToEnable;

    public virtual void Transition(BaseController controller)
    {
        controller.SwitchStates(destinationState.stateName);
        controller.GetGridReference().gameManager.hud.DisableAllElements();
        foreach (string hudElementName in hudElementsToEnable)
        {
            controller.GetGridReference().gameManager.hud.EnableHudElementByName(hudElementName);
        }
    }

    public abstract bool CheckCondition(BaseController controller);
}
