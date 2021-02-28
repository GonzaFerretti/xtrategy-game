using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Show Info Prompt")]
public class TSShowInfo : TutorialStep
{
    [SerializeField] float secondsDuration;
    [SerializeField] TutorialStepInfoPromptConfig infoPromptConfig;
    float endTimestamp;
    [SerializeField] bool endOnInteraction;

    public override void OnEnter()
    {
        Debug.Log("Previous shouldExit value: " + name + " " + shouldExit);

        endTimestamp = Time.time + secondsDuration;

        tutorialManager.HudManager.ShowPrompt(infoPromptConfig);

        if (endOnInteraction)
        {
            tutorialManager.GetGM().OnTutorialEvent += OnInteraction;
        }
    }

    public override bool ShouldExit()
    {
        return Time.time > endTimestamp || base.ShouldExit();
    }

    public void OnInteraction(string interaction)
    {
        Debug.Log("Exited " + name + " from delegate");
        QuickExit();
    }

    public override void OnExit()
    {
        tutorialManager.HudManager.RemoveCurrentPrompt();
        tutorialManager.GetGM().OnTutorialEvent -= OnInteraction;
    }
}

[System.Serializable]
public struct TutorialStepInfoPromptConfig
{
    public string promptText;
    public Vector2Percent percentPosition;
    public TSPromptArrowDirection arrowDirection;
}

[System.Serializable]
public struct Vector2Percent
{
    [Range(0, 1)]
    public float X;
    [Range(0, 1)]
    public float Y;
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
    none,
}
