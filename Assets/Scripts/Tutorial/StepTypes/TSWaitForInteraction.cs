using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Wait for Interaction")]
public class TSWaitForInteraction : TutorialStep
{
    [Header("Interaction Settings")]

    [SerializeField] string interactionIdentifier;
    [SerializeField] bool cleanInteractionsOnExit;
    [SerializeField] bool cleanInteractionsOnEnter;

    [SerializeField] List<Vector3Int> allowedTiles;

    public override void OnEnter()
    {
        if (cleanInteractionsOnEnter)
            tutorialManager.ClearInteractions();

        tutorialManager.GetGM().GetPlayer().tilesInteractionWhiteList = allowedTiles;
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
