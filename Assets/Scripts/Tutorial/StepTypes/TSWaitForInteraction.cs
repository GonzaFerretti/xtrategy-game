using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Wait for Interaction")]
public class TSWaitForInteraction : TutorialStep
{
    [SerializeField] string interactionIdentifier;
    [SerializeField] bool cleanInteractionsOnExit;
    [SerializeField] bool cleanInteractionsOnEnter;

    public override void OnEnter()
    {
        if (cleanInteractionsOnEnter)
            tutorialManager.ClearInteractions();
    }

    public override void OnExit()
    {
        if (cleanInteractionsOnExit)
            tutorialManager.ClearInteractions();
    }

    public override bool ShouldExit()
    {
        return tutorialManager.CheckInteraction(interactionIdentifier);
    }
}
