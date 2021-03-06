﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/Attack Types/Plant Bomb")]
public class AttackTypePlantBomb : AttackType
{
    public override IEnumerator ExecuteAttack(Vector3Int coordinatesToAttack, Unit attackingUnit)
    {
        attackingUnit.anim.Play("Attack");
        attackingUnit.PlaySound(attackingUnit.attributes.mainAttack.attackSound);

        yield return new WaitForSeconds(1);
        attackingUnit.anim.SetTrigger("endCurrentAnim");
        attackingUnit.TryConsumeBuff("attackBoost");
        yield return attackingUnit.StartCoroutine(attackingUnit.grid.CreateMine(attackingUnit.owner, coordinatesToAttack, attackingUnit));
        attackingUnit.attackState = CurrentActionState.ended;
    }

    public override bool CheckPossibleTarget(PlayerController controller)
    {
        controller.CheckUnitUISwitch();
        if (controller.GetObjectUnderMouse(out GameGridCell cell, 1 << LayerMask.NameToLayer("GroundBase"),true))
        {
            Vector3Int target = cell.GetCoordinates();

            if (!controller.currentlySelectedUnit.possibleAttacks.Contains(target)) return false;

            if (controller.GetGridReference().GetUnitAtCoordinates(target) != null) return false;

            if (controller.GetGridReference().CheckMineProximity(out int dmg, target)) return false;

            controller.currentlySelectedUnit.attackState = CurrentActionState.inProgress;
            controller.GetGridReference().EnableCellIndicator(target, GridIndicatorMode.possibleMine, true);
            controller.StartCoroutine(ExecuteAttack(target, controller.currentlySelectedUnit));
            return true;
        }
        return false;
    }

    public override void CheckAdditionalCellIndicatorsConditions(IEnumerable<Vector3Int> indicatorsToCheck, GameGridManager grid, PlayerController controller)
    {
        grid.EnableCellIndicators(indicatorsToCheck, GridIndicatorMode.possibleMine);
    }
}
