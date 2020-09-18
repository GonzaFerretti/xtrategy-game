﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditorInternal;
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

    public virtual void Init(BaseController controller)
    {
        this.controller = controller;
    }
}
