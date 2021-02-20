using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Wait for Interaction")]
public class TSWaitForInteraction : TutorialStep
{
    [SerializeField] string interactionIdentifier;

    public override void OnEnter()
    {
        tutorialManager.ClearInteractions();
    }

    public override void OnExit()
    {
        tutorialManager.ClearInteractions();
    }

    public override bool ShouldExit()
    {
        return tutorialManager.CheckInteraction(interactionIdentifier);
    }
}
