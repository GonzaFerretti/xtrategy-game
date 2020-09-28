using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/AiBehaviour/Melee")]
public class AIBehaviourMeleeUnit : AIBehaviour
{
    public override IEnumerator ExecuteBehaviour(AIController controller, Unit actingUnit)
    {
        AsyncAIActionResult firstAttackAtempt = controller.GenerateNewAIActionResult();
        yield return controller.StartCoroutine(controller.AttemptAttack(actingUnit, firstAttackAtempt.id));
        if (firstAttackAtempt.endedSuccesfully)
        {
            yield break;
        }
        else
        {
            AsyncAIActionResult movementAction = controller.GenerateNewAIActionResult();
            yield return controller.StartCoroutine(controller.MoveTowardsClosestEnemy(actingUnit));
            
            
            AsyncAIActionResult secondAttackAtempt = controller.GenerateNewAIActionResult();
            yield return controller.StartCoroutine(controller.AttemptAttack(actingUnit, secondAttackAtempt.id));
        }
    }
}
