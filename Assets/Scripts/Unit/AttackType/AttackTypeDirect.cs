using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/Attack Types/Direct")]
public class AttackTypeDirect : AttackType
{
    public override IEnumerator ExecuteAttack(Vector3Int coordinatesToAttack, Unit attackingUnit)
    {
        Unit enemyToAttack = attackingUnit.owner.GetGridReference().GetUnitAtCoordinates(coordinatesToAttack);

        attackingUnit.anim.Play("Attack");
        attackingUnit.PlaySound(attackingUnit.attributes.mainAttack.attackSound);
        attackingUnit.model.transform.forward = (enemyToAttack.transform.position - attackingUnit.transform.position).normalized;
        yield return new WaitForSeconds(1);
        attackingUnit.anim.SetTrigger("endCurrentAnim");
        attackingUnit.TryConsumeBuff("attackBoost");
        AttackAction(attackingUnit, enemyToAttack);
        attackingUnit.grid.gameManager.TriggerTutorialEvent("attackTargetSelect");
        attackingUnit.attackState = CurrentActionState.ended;
    }

    public override void AttackAction(Unit attackingUnit, Unit attackedUnit, AttackAttributes attributesToUse = null)
    {
        if (!attributesToUse)
        {
            attributesToUse = attackingUnit.attributes.mainAttack;
        }
        attackedUnit.TakeDamage(CalculateFinalDamage(attackingUnit, false, attributesToUse), attackingUnit.GetCoordinates(), true);
    }

    public override bool CheckPossibleTarget(PlayerController controller)
    {
        controller.CheckUnitUISwitch();
        if (controller.GetObjectUnderMouse(out Unit unitSelected, 1 << LayerMask.NameToLayer("Unit")))
        {
            if (unitSelected == controller.currentlySelectedUnit) return false;
            Vector3Int unitPosition = unitSelected.GetCoordinates();
            if (controller.currentlySelectedUnit.possibleAttacks.Contains(unitPosition))
            {
                controller.currentlySelectedUnit.attackState = CurrentActionState.inProgress;
                controller.GetGridReference().EnableCellIndicator(unitPosition, GridIndicatorMode.possibleAttack, true);
                controller.StartCoroutine(ExecuteAttack(unitPosition,controller.currentlySelectedUnit));
                return true;
            }
        }
        return false;
    }

    public override void CheckAdditionalCellIndicatorsConditions(IEnumerable<Vector3Int> coordinatesToCheck, GameGridManager grid, PlayerController controller) 
    {
        foreach (var coordinates in coordinatesToCheck)
        {
            var cell = grid.GetCellAtCoordinate(coordinates);
            Unit unit = grid.GetUnitAtCoordinates(coordinates);
            if (unit && unit.owner != controller)
            {
                grid.EnableCellIndicator(coordinates, GridIndicatorMode.possibleAttack);
            }
        }
    }
}
