﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/AiBehaviour/Boss")]
public class AIBehaviourBoss : AIBehaviour
{
    [SerializeField]
    int turnsBeforeAttack;

    [SerializeField] Color startColor;
    [SerializeField] Color warningColor;

    public override IEnumerator ExecuteBehaviour(AIController controller, Unit actingUnit)
    {
        GameGridManager grid = controller.GetGridReference();
        AISavedData savedData = new AISavedData() { lastAttackTurn = 0 };
        if (controller.AIUnitsSavedData.ContainsKey(actingUnit)) savedData = controller.AIUnitsSavedData[actingUnit];

        int currentTurnsBeforeAttack = grid.gameManager.GetTurnNumber() - savedData.lastAttackTurn;

        if (currentTurnsBeforeAttack <= turnsBeforeAttack)
        {
            float currentChargeProgress = ((currentTurnsBeforeAttack) * 1.0f) / ((turnsBeforeAttack) * 1.0f);
            if (actingUnit.mainMaterial)
            {
                Color currentAlertLevelColor = Color.Lerp(startColor, warningColor, currentChargeProgress);
                actingUnit.mainMaterial.SetColor("_EmissionColor", currentAlertLevelColor);
                if (currentTurnsBeforeAttack == turnsBeforeAttack)
                {
                    actingUnit.mainMaterial.SetColor("_Color", currentAlertLevelColor);
                }
                else if (currentTurnsBeforeAttack == 1) actingUnit.mainMaterial.SetColor("_Color", Color.white);
            }

            yield return controller.StartCoroutine(controller.MoveTowardsClosestTarget(actingUnit, controller.GetUnitsFromOthers()));
        }
        else
        {
            BossAttributes bossData = actingUnit.attributes as BossAttributes;

            AsyncAIActionResult firstAttack = controller.GenerateNewAIActionResult();
            yield return controller.StartCoroutine(controller.AttemptInteractWithLowestHPTarget(actingUnit, firstAttack.id, shouldChangeAttackState: false));

            AsyncAIActionResult secondAttack = controller.GenerateNewAIActionResult();
            yield return controller.StartCoroutine(controller.AttemptInteractWithLowestHPTarget(actingUnit, secondAttack.id, attackToUse: bossData.secondAttack, shouldChangeAttackState: false));

            AsyncAIActionResult thirdAttack = controller.GenerateNewAIActionResult();
            yield return controller.StartCoroutine(controller.AttemptInteractWithLowestHPTarget(actingUnit, thirdAttack.id, attackToUse: bossData.thirdAttack));

            AISavedData newSavedData = new AISavedData()
            {
                lastAttackTurn = grid.gameManager.GetTurnNumber()
            };

            if (controller.AIUnitsSavedData.ContainsKey(actingUnit))
                controller.AIUnitsSavedData[actingUnit] = newSavedData;
            else
                controller.AIUnitsSavedData.Add(actingUnit, newSavedData);
        }
    }
}
