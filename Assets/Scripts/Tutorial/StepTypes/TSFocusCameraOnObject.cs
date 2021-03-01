using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Focus Camera On Object")]
public class TSFocusCameraOnObject : TutorialStep
{
    [Header("Focus Camera Settings")]

    [SerializeField] string objectNameToFocus;
    [SerializeField] float waitAfterFinishing;

    public override void OnEnter()
    {
        GameObject objectToFocus = GameObject.Find(objectNameToFocus);

        if (objectToFocus)
        {
            tutorialManager.GetCameraController().OnCameraTargetFollowEnd += OnFocusEnd;
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

    public void OnFocusEnd()
    {
        tutorialManager.StartCoroutine(WaitAfterFinishing());
    }

    public IEnumerator WaitAfterFinishing()
    {
        yield return new WaitForSeconds(waitAfterFinishing);
        QuickExit();
    }
}
