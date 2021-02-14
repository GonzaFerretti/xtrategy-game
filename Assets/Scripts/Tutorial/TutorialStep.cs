﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TutorialStep : ScriptableObject
{
    public bool shouldBlockEntireInterface;

    [HideInInspector] public TutorialManager tutorialManager;

    public abstract void OnEnter();
    public abstract void OnExit();

    public List<string> disabledUiElements;
    public abstract bool ShouldExit();
}