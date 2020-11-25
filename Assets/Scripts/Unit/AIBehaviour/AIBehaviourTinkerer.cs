using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/AiBehaviour/Tinkerer")]

public class AIBehaviourTinkerer : AIBehaviour
{
    [SerializeField] int turnsBeforeAttack;
    [SerializeField] int distanceToStartMining;

    public override IEnumerator ExecuteBehaviour(AIController controller, Unit actingUnit)
    {
        if (actingUnit.currentCovers.Count == 0)
        {
            AsyncAIActionResult moveToCloserCover = controller.GenerateNewAIActionResult();
            yield return controller.StartCoroutine(controller.MoveTowardsCoverCloseToEnemy(actingUnit, moveToCloserCover.id));
            if (moveToCloserCover.endedSuccesfully)
            {
                AsyncAIActionResult SecondAttackAttempt = controller.GenerateNewAIActionResult();
                yield return controller.StartCoroutine(controller.AttemptAttack(actingUnit, SecondAttackAttempt.id));
            }
        }

        GameGridManager grid = controller.GetGridReference();

        if (controller.AIUnitsSavedData.ContainsKey(actingUnit))
        {
            AISavedData savedData = controller.AIUnitsSavedData[actingUnit];
            if (grid.gameManager.GetTurnNumber() - savedData.lastAttackTurn < turnsBeforeAttack) yield break;
        }

        bool hasEnemiesClose = false;
        foreach (var unit in grid.gameManager.allUnits)
        {
            if (actingUnit.owner == unit.owner) continue;
            if (grid.GetAttackRangeDistance(actingUnit.GetCoordinates(), unit.GetCoordinates()) > distanceToStartMining) continue;
            hasEnemiesClose = true;
            break;
        }

        if (hasEnemiesClose)
        {
            AsyncRangeQuery attackRangeQuery = actingUnit.StartAttackRangeQuery();
            while (!attackRangeQuery.hasFinished)
            {
                yield return null;
            }

            foreach (var possibleAttack in attackRangeQuery.cellsInRange)
            {
                if (!grid.minedPositionList.ContainsKey(possibleAttack))
                {
                    grid.CreateMine(actingUnit.owner, possibleAttack);
                    actingUnit.attackState = CurrentActionState.ended;
                    if (controller.AIUnitsSavedData.ContainsKey(actingUnit))
                    {
                        AISavedData savedData = controller.AIUnitsSavedData[actingUnit];
                        savedData.lastAttackTurn = grid.gameManager.GetTurnNumber();

                        controller.AIUnitsSavedData[actingUnit] = savedData;
                    }
                    else
                    {
                        AISavedData savedData = new AISavedData()
                        {
                            lastAttackTurn = grid.gameManager.GetTurnNumber()
                        };

                        controller.AIUnitsSavedData.Add(actingUnit, savedData);
                    }
                    break;
                }
            }

            attackRangeQuery.End();
        }
    }
}
