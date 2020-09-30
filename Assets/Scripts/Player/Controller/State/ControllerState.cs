using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerState : ScriptableObject
{
    public string stateName;
    protected BaseController controller;
    [SerializeField] ControllerStateTransition[] transitions;

    public virtual void OnUpdate()
    {
        CheckTransitions();
    }

    protected void CheckTransitions()
    {
        foreach (ControllerStateTransition transition in transitions)
        {
            if (transition.CheckCondition(controller))
            {
                transition.Transition(controller);
            }
        }
    }

    public void ForceFirstTransition()
    {
        transitions[0].Transition(controller);
    }

    public virtual void Init(BaseController controller)
    {
        this.controller = controller;
    }
}
