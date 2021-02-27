using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerState : ScriptableObject
{
    public delegate void ControllerExitDelegate(GridIndicatorMode mode, Vector3Int position);
    public ControllerExitDelegate controllerExitDelegate;
    public string stateName;
    protected BaseController controller;
    [SerializeField] ControllerStateTransition[] transitions;

    public virtual void OnUpdate()
    {
        CheckTransitions();
    }

    public virtual void OnTransitionIn()
    {
    }

    public virtual void OnTransitionOut()
    {

    }

    protected void CheckTransitions()
    {
        foreach (ControllerStateTransition transition in transitions)
        {
            if (transition.CheckCondition(controller))
            {
                OnTransitionOut();
                transition.Transition(controller);
                controller.StartCoroutine(controller.WaitForTransitionIn());
                break;
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
