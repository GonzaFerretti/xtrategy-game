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
            if (!actingUnit) yield break;
            AsyncAIActionResult secondDisableAttempt = controller.GenerateNewAIActionResult();
            yield return controller.StartCoroutine(controller.AttemptToDetonateMine(actingUnit, secondDisableAttempt.id));
            if (secondDisableAttempt.endedSuccesfully)
            {
                AsyncAIActionResult moveToCloserCover = controller.GenerateNewAIActionResult();
                if (!actingUnit) yield break;
                yield return controller.StartCoroutine(controller.MoveTowardsCoverCloseToEnemy(actingUnit, moveToCloserCover.id));
            }
        }
    }
}
