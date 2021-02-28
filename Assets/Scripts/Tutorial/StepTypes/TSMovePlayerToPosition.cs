using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Move Player To Position")]
public class TSMovePlayerToPosition : TutorialStep
{
    [Header("Move Player Settings")]
    [SerializeField] Vector3Int newPositionForAllyUnit;

    public override void OnEnter()
    {
        tutorialManager.tutorialUnit.UpdateCell(newPositionForAllyUnit);

        QuickExit();
    }
}
