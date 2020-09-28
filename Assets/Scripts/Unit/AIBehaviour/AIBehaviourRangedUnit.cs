using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/AiBehaviour/Ranged")]
public class AIBehaviourRangedUnit : AIBehaviour
{
    // I should use a WaitForActionAttempt(IEnumerator) coroutine instead of doing these horrible repetitions.
    public override IEnumerator ExecuteBehaviour(AIController controller, Unit actingUnit)
    {
        if (actingUnit.currentCovers.Count > 0)
        {
            AsyncAIActionResult attackFromCover = controller.GenerateNewAIActionResult();
            yield return controller.StartCoroutine(controller.AttemptAttack(actingUnit, attackFromCover.id));
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
                    yield return controller.StartCoroutine(controller.AttemptAttack(actingUnit, SecondAttackAttempt.id));
                }
            }
        }
        else
        {
            AsyncAIActionResult moveToCloserCover = controller.GenerateNewAIActionResult();
            yield return controller.StartCoroutine(controller.MoveTowardsCoverCloseToEnemy(actingUnit, moveToCloserCover.id));
            if (moveToCloserCover.endedSuccesfully)
            {
                AsyncAIActionResult SecondAattackAttemptFromCover = controller.GenerateNewAIActionResult();
                yield return controller.StartCoroutine(controller.AttemptAttack(actingUnit, SecondAattackAttemptFromCover.id));
            }
        }
    }
}
