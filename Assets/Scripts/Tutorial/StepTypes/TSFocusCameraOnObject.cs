using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Focus Camera On Object")]
public class TSFocusCameraOnObject : TutorialStep
{
    [SerializeField] string objectNameToFocus;

    public override void OnEnter()
    {
        GameObject objectToFocus = GameObject.Find(objectNameToFocus);

        if (objectToFocus)
        {
            tutorialManager.GetCameraController().OnCameraTargetFollowEnd += QuickExit;
            tutorialManager.GetCameraController().SetFollowTarget(objectToFocus.transform);
            tutorialManager.GetCameraController().lockUserMovement = true;
        }
        else
            QuickExit();
    }

    public override void OnExit()
    {
        tutorialManager.GetCameraController().OnCameraTargetFollowEnd -= QuickExit;
        tutorialManager.GetCameraController().lockUserMovement = false;
    }

    public override bool ShouldExit()
    {
        return base.ShouldExit();
    }
}
