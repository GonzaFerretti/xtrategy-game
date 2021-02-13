using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/AiBehaviour/Witch Doctor")]
public class AIBehaviourWitchDoctor : AIBehaviour
{
    [SerializeField] [Range(0,1)] float hpThreshold;

    public override IEnumerator ExecuteBehaviour(AIController controller, Unit actingUnit)
    {
        yield return null;
        if (GetWoundedAllies(controller, out List<Unit> woundedAllies))
        {
            AsyncAIActionResult healAttempt = controller.GenerateNewAIActionResult();
            yield return controller.StartCoroutine(controller.AttemptInteractWithLowestHPTarget(actingUnit, healAttempt.id, TargetType.AllyOnly));
            if (healAttempt.endedSuccesfully)
            {
                if (GetWoundedAllies(controller, out woundedAllies))
                {
                    yield return controller.StartCoroutine(controller.MoveTowardsClosestTarget(actingUnit, woundedAllies));
                }
                else
                {
                    AsyncAIActionResult moveToCoverClosestToEnemy = controller.GenerateNewAIActionResult();
                    yield return controller.StartCoroutine(controller.MoveTowardsCoverCloseToEnemy(actingUnit, moveToCoverClosestToEnemy.id));
                }
            }
            else
            {
                yield return controller.StartCoroutine(controller.MoveTowardsClosestTarget(actingUnit, woundedAllies));
                AsyncAIActionResult healAttempt2 = controller.GenerateNewAIActionResult();
                yield return controller.StartCoroutine(controller.AttemptInteractWithLowestHPTarget(actingUnit, healAttempt2.id, TargetType.AllyOnly));
                if (!healAttempt2.endedSuccesfully)
                {
                    yield return controller.StartCoroutine(controller.AttemptInteractWithLowestHPTarget(actingUnit, controller.GenerateNewAIActionResult().id));
                }
            }
        }
        else
        {
            AsyncAIActionResult attackAttempt = controller.GenerateNewAIActionResult();
            yield return controller.StartCoroutine(controller.AttemptInteractWithLowestHPTarget(actingUnit, attackAttempt.id));
            AsyncAIActionResult moveToCoverClosestToEnemy = controller.GenerateNewAIActionResult();
            yield return controller.StartCoroutine(controller.MoveTowardsCoverCloseToEnemy(actingUnit, moveToCoverClosestToEnemy.id));
            if (!attackAttempt.endedSuccesfully && moveToCoverClosestToEnemy.endedSuccesfully)
            {
                AsyncAIActionResult attackAttempt2 = controller.GenerateNewAIActionResult();
                yield return controller.StartCoroutine(controller.AttemptInteractWithLowestHPTarget(actingUnit, attackAttempt2.id));
            }
        }
    }

    bool GetWoundedAllies(AIController controller, out List<Unit> woundedAllies)
    {
        woundedAllies = new List<Unit>();
        foreach (Unit unit in controller.unitsControlled)
        {
            if (unit.IsDamaged(hpThreshold))
            {
                woundedAllies.Add(unit);
            }
        }
        return woundedAllies.Count > 0;
    }
}