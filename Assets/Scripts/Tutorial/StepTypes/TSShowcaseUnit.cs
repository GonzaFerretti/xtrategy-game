using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Showcase Unit")]
public class TSShowcaseUnit : TutorialStep
{
    [Header("Unit spawn config")]
    [SerializeField] UnitAttributes unitPrefab;
    [SerializeField] string newElementName;
    [SerializeField] Vector3Int coordinates;

    [Header("Focus config")]
    [SerializeField] float deadTimeAfterFocus;

    [Header("Info prompt config")]
    [SerializeField] float promptDuration;
    [SerializeField] TutorialStepInfoPromptConfig infoPromptConfig;
    
    float endTimestamp;
    bool promptStarted = false;

    public override void OnEnter()
    {
        // Spawn
        TutorialElementsSpawnData spawnData = (unitPrefab as ITutorialElementSpawnData).GetTutorialSpawnData();

        spawnData.coordinates = coordinates;
        spawnData.isPlayerOwned = false;

        GameObject newElement = tutorialManager.GetGM().SpawnTutorialElement(spawnData, TutorialElementType.unit);

        newElement.name = newElementName;

        tutorialManager.spawnedTutorialElementsData.Add(new SpawnedTutorialElementData { element = newElement, type = TutorialElementType.unit });

        // Focus

        if (newElement)
        {
            tutorialManager.GetCameraController().OnCameraTargetFollowEnd += OnFocusEnd;
            tutorialManager.GetCameraController().SetFollowTarget(newElement.transform);
            tutorialManager.GetCameraController().lockUserMovement = true;
        }
    }

    public override void OnExit()
    {
        tutorialManager.GetCameraController().lockUserMovement = false;
        tutorialManager.HudManager.RemoveCurrentPrompt();
    }

    public void OnFocusEnd()
    {
        tutorialManager.StartCoroutine(WaitAfterFinishing());
    }

    public IEnumerator WaitAfterFinishing()
    {
        yield return new WaitForSeconds(deadTimeAfterFocus);

        // End focus
        tutorialManager.GetCameraController().OnCameraTargetFollowEnd -= OnFocusEnd;

        // Start prompt
        endTimestamp = Time.time + promptDuration;
        promptStarted = true;
        tutorialManager.HudManager.ShowPrompt(infoPromptConfig);
    }

    public override bool ShouldExit()
    {
        return Time.time > endTimestamp && promptStarted;
    }
}
