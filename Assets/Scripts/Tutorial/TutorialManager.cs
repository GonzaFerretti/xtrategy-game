using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] TutorialStep[] tutorialSteps;

    public List<SpawnedTutorialElementData> spawnedTutorialElementsData;

    [HideInInspector] public HUDManager HudManager;

    public Unit tutorialUnit;

    List<string> interactionCache = new List<string>();

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
        spawnedTutorialElementsData = new List<SpawnedTutorialElementData>();
        HudManager = FindObjectOfType<HUDManager>();
        GetGM().OnTutorialEvent += OnInteraction;
        for (int i = 0; i < tutorialSteps.Length; i++)
        {
            tutorialSteps[i] = Instantiate(tutorialSteps[i]);
            tutorialSteps[i].tutorialManager = this;
        }
        StartCoroutine(CycleThroughSteps());
    }

    IEnumerator CycleThroughSteps()
    {
        while (currentIndex < tutorialSteps.Length)
        {
            TutorialStep currentStep = tutorialSteps[currentIndex];
            Debug.Log(currentStep.name);
            if (currentStep.shouldBlockEntireInterface) SetInterfaceLock(true);
            DisableUIElements(currentStep.disabledUiElements);
            currentStep.OnEnter();
            yield return new WaitForStepExit(currentStep);
            currentStep.OnExit();
            if (shouldKeepTheInterfaceLocked(currentStep)) SetInterfaceLock(false);

            currentIndex++;
        }
        Debug.Log("Finished!");
    }

    bool shouldKeepTheInterfaceLocked(TutorialStep currentStep)
    {
        int nextIndex = currentIndex + 1;
        bool currentIsBlocked = currentStep.shouldBlockEntireInterface;
        bool shouldKeepItBlocked = (nextIndex < tutorialSteps.Length && !tutorialSteps[nextIndex].shouldBlockEntireInterface && currentIsBlocked)
            || (nextIndex >= tutorialSteps.Length && currentIsBlocked);
        return shouldKeepItBlocked;
    }

    void SetInterfaceLock(bool newStatus)
    {
        if (GetGM().OnInterfaceLock != null)
        {
            GetGM().OnInterfaceLock.Invoke(newStatus);
        }
    }

    void DisableUIElements(List<string> elementIdentifiers)
    {

    }

    public void OnInteraction(string identifier)
    {
        if (!interactionCache.Contains(identifier))
            interactionCache.Add(identifier);
    }

    public bool CheckInteraction(string identifier, bool shouldConsume = true)
    {
        bool wasActivated = interactionCache.Contains(identifier);
        if (wasActivated && shouldConsume)
            interactionCache.Remove(identifier);
        return wasActivated;
    }

    public void ClearInteractions()
    {
        interactionCache.Clear();
    }

    public void CleanSpawnedElements()
    {
        while (spawnedTutorialElementsData.Count > 0)
        {
            if (spawnedTutorialElementsData[0].element)
                GetGM().DestroyTutorialElement(spawnedTutorialElementsData[0].element, spawnedTutorialElementsData[0].type);
            spawnedTutorialElementsData.RemoveAt(0);
        }
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
