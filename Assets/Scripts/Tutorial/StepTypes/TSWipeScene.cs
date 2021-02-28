using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Wipe Scene")]
public class TSWipeScene : TutorialStep
{
    public override void OnEnter()
    {
        tutorialManager.CleanSpawnedElements();

        QuickExit();
    }
}
