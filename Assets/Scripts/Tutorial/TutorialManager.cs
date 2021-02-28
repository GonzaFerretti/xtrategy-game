using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] TutorialStep[] tutorialSteps;

    public List<SpawnedTutorialElementData> spawnedTutorialElements;

    [HideInInspector] public HUDManager HudManager;

    public Unit tutorialUnit;

    List<string> buttonPressStates = new List<string>();

    int currentIndex = 0;

    public CameraController GetCameraController()
    {
        return GetGM().camController;
    }

    public GameManager GetGM()
    {
        return HudManager.gm;
    }

    public void StartTutorial()
    {
        tutorialUnit = FindObjectOfType<Unit>();
        HudManager = FindObjectOfType<HUDManager>();
        foreach (var step in tutorialSteps)
        {
            step.tutorialManager = this;
        }
        StartCoroutine(CycleThroughSteps());
    }

    IEnumerator CycleThroughSteps()
    {
        while (currentIndex < tutorialSteps.Length)
        {
            TutorialStep currentStep = tutorialSteps[currentIndex];
            if (currentStep.shouldBlockEntireInterface) SetInterfaceLock(true);
            DisableUIElements(currentStep.disabledUiElements);
            currentStep.OnEnter();
            yield return new WaitForStepExit(currentStep);
            currentStep.OnExit();
            if (currentStep.shouldBlockEntireInterface) SetInterfaceLock(false);
            currentIndex++;
        }
    }

    void SetInterfaceLock(bool newStatus)
    {

    }

    void DisableUIElements(List<string> elementIdentifiers)
    {

    }

    public void OnInteraction(string identifier)
    {
        if (!buttonPressStates.Contains(identifier))
            buttonPressStates.Add(identifier);
    }

    public bool CheckInteraction(string identifier, bool shouldConsume = true)
    {
        bool wasActivated = buttonPressStates.Contains(identifier);
        if (wasActivated && shouldConsume)
            buttonPressStates.Remove(identifier);
        return wasActivated;
    }

    public void ClearInteractions()
    {
        buttonPressStates.Clear();
    }
}

public class WaitForStepExit : CustomYieldInstruction
{
    TutorialStep step;

    public override bool keepWaiting
    {
        get
        {
            return !step.ShouldExit();
        }
    }

    public WaitForStepExit(TutorialStep step)
    {
        this.step = step;
    }
}
