using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ControllerStateTransition : ScriptableObject
{
    public ControllerState destinationState;
    public List<string> hudElementsToEnable;
    public bool shouldShowUnitCycleHUD;

    public virtual void Transition(BaseController controller)
    {
        //Debug.Log("Transitioned from state " + controller.GetCurrentStateName() + " to state " + destinationState.stateName + " through transition " + name);
        controller.SwitchStates(destinationState.stateName);
        controller.GetGridReference().gameManager.hud.DisableAllElements();
        foreach (string hudElementName in hudElementsToEnable)
        {
            controller.GetGridReference().gameManager.hud.EnableHudElementByName(hudElementName);
        }
        
        if (shouldShowUnitCycleHUD)
            UpdateUnitCycleHUDVisibility(controller);
    }

    public void UpdateUnitCycleHUDVisibility(BaseController controller)
    {
        if (controller.HasMultipleUnits())
        {
            controller.GetGridReference().gameManager.hud.EnableHudElementByName("NextUnit");
            controller.GetGridReference().gameManager.hud.EnableHudElementByName("PreviousUnit");
        }
    }

    public abstract bool CheckCondition(BaseController controller);
}
