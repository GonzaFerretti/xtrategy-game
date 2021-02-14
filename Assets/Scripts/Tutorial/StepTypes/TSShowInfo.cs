using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Show Info Prompt")]
public class TSShowInfo : TutorialStep
{
    [SerializeField] float secondsDuration;
    [SerializeField] TutorialStepInfoPromptConfig infoPromptConfig;
    float endTimestamp;

    public override void OnEnter()
    {
        endTimestamp = Time.time + secondsDuration;

        tutorialManager.HudManager.ShowPrompt(infoPromptConfig);
    }

    public override bool ShouldExit()
    {
        return Time.time > endTimestamp;
    }

    public override void OnExit()
    {
        tutorialManager.HudManager.RemoveCurrentPrompt();
    }
}

[System.Serializable]
public struct TutorialStepInfoPromptConfig
{
    public string promptText;
    public Vector2 positionInScreen;
    public TSPromptArrowDirection arrowDirection;
}

public enum TSPromptArrowDirection
{
    top,
    topleft,
    botleft,
    bot,
    botright,
    topright,
    left,
    right,
}
