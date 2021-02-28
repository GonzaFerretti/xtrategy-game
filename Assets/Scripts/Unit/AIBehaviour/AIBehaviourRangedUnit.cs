using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/AiBehaviour/Ranged")]
public class AIBehaviourRangedUnit : AIBehaviour
{
    public override IEnumerator ExecuteBehaviour(AIController controller, Unit actingUnit)
    {
        if (actingUnit.currentCovers.Count > 0)
        {
            AsyncAIActionResult attackFromCover = controller.GenerateNewAIActionResult();
            yield return controller.StartCoroutine(controller.AttemptInteractWithLowestHPTarget(actingUnit, attackFromCover.id));
            if (attackFromCover.endedSuccesfully)
            {
                yield break;
            }
            else
            {
                AsyncAIActionResult moveToCloserCover = controller.GenerateNewAIActionResult();
                yield return controller.StartCoroutine(controller.MoveTowardsCoverCloseToEnemy(actingUnit, moveToCloserCover.id));
                if (moveToCloserCover.endedSuccesfully)
                {
                    AsyncAIActionResult SecondAttackAttempt = controller.GenerateNewAIActionResult();
                    yield return controller.StartCoroutine(controller.AttemptInteractWithLowestHPTarget(actingUnit, SecondAttackAttempt.id));
                }
                if (!actingUnit) yield break;
            }
        }
        else
        {
            AsyncAIActionResult moveToCloserCover = controller.GenerateNewAIActionResult();
            yield return controller.StartCoroutine(controller.MoveTowardsCoverCloseToEnemy(actingUnit, moveToCloserCover.id));
            if (moveToCloserCover.endedSuccesfully)
            {
                AsyncAIActionResult SecondAattackAttemptFromCover = controller.GenerateNewAIActionResult();
                yield return controller.StartCoroutine(controller.AttemptInteractWithLowestHPTarget(actingUnit, SecondAattackAttemptFromCover.id));
            }
            else
            {
                AsyncAIActionResult AttackAttemptWithoutCover = controller.GenerateNewAIActionResult();
                yield return controller.StartCoroutine(controller.AttemptInteractWithLowestHPTarget(actingUnit, AttackAttemptWithoutCover.id));
            }
            if (!actingUnit) yield break;
        }
    }
}
