using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TutorialStep : ScriptableObject
{
    [Header("Base tutorial step Settings")]

    public bool shouldBlockEntireInterface;

    [HideInInspector] protected bool shouldExit = false;

    [HideInInspector] public TutorialManager tutorialManager;

    public abstract void OnEnter();
    public virtual void OnExit() { }

    public List<string> disabledUiElements;
    public virtual bool ShouldExit()
    {
        return shouldExit;
    }

    protected void QuickExit()
    {
        shouldExit = true;
    }
}
