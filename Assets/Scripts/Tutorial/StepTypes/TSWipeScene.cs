using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Wipe Scene")]
public class TSWipeScene : TutorialStep
{
    [SerializeField] Vector3Int newPositionForAllyUnit;

    public override void OnEnter()
    {
        List<SpawnedTutorialElementData> tutorialElements = tutorialManager.spawnedTutorialElementsData;
        while (tutorialElements.Count > 0)
        {
            tutorialManager.GetGM().DestroyTutorialElement(tutorialElements[0].element, tutorialElements[0].type);
            tutorialManager.spawnedTutorialElementsData.RemoveAt(0);
        }

        tutorialManager.tutorialUnit.UpdateCell(newPositionForAllyUnit);

        QuickExit();
    }

    public override void OnExit()
    {
    }
}
