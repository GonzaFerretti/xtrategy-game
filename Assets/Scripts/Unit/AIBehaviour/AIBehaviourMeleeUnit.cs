using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/AiBehaviour/Melee")]
public class AIBehaviourMeleeUnit : AIBehaviour
{
    public override IEnumerator ExecuteBehaviour(AIController controller, Unit actingUnit)
    {
        Debug.Log("Starting " + actingUnit.name);
        AsyncAIActionResult firstAttackAtempt = controller.GenerateNewAIActionResult();
        yield return controller.StartCoroutine(AttemptAttack(controller, actingUnit, firstAttackAtempt.id));
        if (firstAttackAtempt.endedSuccesfully)
        {
            Debug.Log("Unit " + actingUnit.name + " attacked");
            yield break;
        }
        else
        {
            AsyncAIActionResult movementAction = controller.GenerateNewAIActionResult();
            yield return controller.StartCoroutine(MoveTowardsClosestEnemy(controller, actingUnit));
            if (movementAction.endedSuccesfully)
            {
                Debug.Log("Unit " + actingUnit.name + " moved");
            }
            

            AsyncAIActionResult secondAttackAtempt = controller.GenerateNewAIActionResult();
            yield return controller.StartCoroutine(AttemptAttack(controller, actingUnit, secondAttackAtempt.id));
            if (secondAttackAtempt.endedSuccesfully)
            {
                Debug.Log("Unit " + actingUnit.name + " attacked (second)");
            }
        }
    }

    IEnumerator AttemptAttack(AIController controller, Unit actingUnit, int id)
    {
         AsyncRangeQuery attackQuery = actingUnit.StartAttackRangeQuery();

        while (!attackQuery.hasFinished)
        {
            yield return null;
        }
        if (attackQuery.cellsInRange.Count > 0)
        {
            Unit lowestHpUnitInRange = controller.GetLowestUnitInAttackRange(attackQuery.cellsInRange);
            if (lowestHpUnitInRange)
            {
                controller.Attack(lowestHpUnitInRange, actingUnit);
                controller.currentActions[id].endedSuccesfully = true;
            }
            else
            {
                controller.currentActions[id].endedSuccesfully = false;
            }
        }
        else
        {
            controller.currentActions[id].endedSuccesfully = false;
        }

        attackQuery.EndQuery();
    }

    IEnumerator MoveTowardsClosestEnemy(AIController controller, Unit actingUnit)
    {
        Vector3Int[] pathToClosestEnemy = controller.GetGridReference().GetBestPathToGetToClosestUnit(actingUnit, controller.GetUnitsFromOthers());
        if (pathToClosestEnemy.Length > 0)
        {
            int movementRange = actingUnit.movementRange;
            if (pathToClosestEnemy.Length > movementRange)
            {
                Vector3Int[] longestPossiblePath = new Vector3Int[movementRange];
                for (int i = 0; i < movementRange; i++)
                {
                    longestPossiblePath[i] = pathToClosestEnemy[i];
                }
                pathToClosestEnemy = longestPossiblePath;
            }
            yield return controller.StartCoroutine(actingUnit.MoveAlongPath(pathToClosestEnemy));
        }
    }
}
