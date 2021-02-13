using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/AiBehaviour/Melee")]
public class AIBehaviourMeleeUnit : AIBehaviour
{
    public override IEnumerator ExecuteBehaviour(AIController controller, Unit actingUnit)
    {
        AsyncAIActionResult firstAttackAtempt = controller.GenerateNewAIActionResult();
        yield return controller.StartCoroutine(controller.AttemptInteractWithLowestHPTarget(actingUnit, firstAttackAtempt.id));
        if (firstAttackAtempt.endedSuccesfully)
        {
            yield break;
        }
        else
        {
            yield return controller.StartCoroutine(controller.MoveTowardsClosestTarget(actingUnit,controller.GetUnitsFromOthers()));
            if (!actingUnit) yield break;
            
            AsyncAIActionResult secondAttackAtempt = controller.GenerateNewAIActionResult();
            yield return controller.StartCoroutine(controller.AttemptInteractWithLowestHPTarget(actingUnit, secondAttackAtempt.id));
        }
    }
}
