using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/AiBehaviour/Boss")]
public class AIBehaviourBoss : AIBehaviour
{
    [SerializeField]
    int turnsBeforeAttack;

    public override IEnumerator ExecuteBehaviour(AIController controller, Unit actingUnit)
    {
        GameGridManager grid = controller.GetGridReference();
        AISavedData savedData = new AISavedData() { lastAttackTurn = 0 };
        if (controller.AIUnitsSavedData.ContainsKey(actingUnit)) savedData = controller.AIUnitsSavedData[actingUnit];

        if (grid.gameManager.GetTurnNumber() - savedData.lastAttackTurn < turnsBeforeAttack)
        {
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

            Debug.Log("First attack: " + firstAttack.endedSuccesfully + " Second attack: " + secondAttack.endedSuccesfully + " Third attack: " + thirdAttack.endedSuccesfully);

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
