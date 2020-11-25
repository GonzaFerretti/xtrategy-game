using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/AiBehaviour/Disabler")]
public class AIBehaviourBombDisabler : AIBehaviour
{
    public override IEnumerator ExecuteBehaviour(AIController controller, Unit actingUnit)
    {
        AsyncAIActionResult firstDisableAttempt = controller.GenerateNewAIActionResult();
        yield return controller.StartCoroutine(controller.AttemptToDetonateMine(actingUnit, firstDisableAttempt.id));
        if (firstDisableAttempt.endedSuccesfully)
        {
            yield break;
        }
        else
        {
            yield return controller.StartCoroutine(controller.MoveTowardsClosestMine(actingUnit));

            AsyncAIActionResult secondDisableAttempt = controller.GenerateNewAIActionResult();
            yield return controller.StartCoroutine(controller.AttemptToDetonateMine(actingUnit, secondDisableAttempt.id));
            if (secondDisableAttempt.endedSuccesfully)
            {
                AsyncAIActionResult moveToCloserCover = controller.GenerateNewAIActionResult();
                yield return controller.StartCoroutine(controller.MoveTowardsCoverCloseToEnemy(actingUnit, moveToCloserCover.id));
            }
        }
    }
}
