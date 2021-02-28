using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Set Enemy Interactions")]
public class TSSetEnemyInteractionLevel : TutorialStep
{
    [Header("Enemy interaction settings")]
    [SerializeField] TutorialEnemyFlags flags;

    public override void OnEnter()
    {
        tutorialManager.GetGM().enemyFlags = flags;
        QuickExit();
    }

}
