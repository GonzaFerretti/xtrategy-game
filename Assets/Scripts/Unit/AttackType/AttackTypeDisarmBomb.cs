﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/Attack Types/Disarm bomb")]
public class AttackTypeDisarmBomb : AttackType
{
    [SerializeField] float centerDamageMultiplier;

    public override IEnumerator ExecuteAttack(Vector3Int coordinatesToAttack, Unit attackingUnit)
    {
        attackingUnit.anim.Play("Attack");
        attackingUnit.PlaySound(attackingUnit.attributes.mainAttack.attackSound);

        yield return new WaitForSeconds(1);
        attackingUnit.anim.SetTrigger("endCurrentAnim");
        attackingUnit.TryConsumeBuff("attackBoost");
        attackingUnit.grid.DetonateMine(coordinatesToAttack, attackingUnit);
        attackingUnit.attackState = CurrentActionState.ended;
    }

    public override bool CheckPossibleTarget(PlayerController controller)
    {
        controller.CheckUnitUISwitch();

        if (controller.GetObjectUnderMouse(out MagicMine mineSelected, 1 << LayerMask.NameToLayer("Mine")))
        {
            Vector3Int target = mineSelected.coordinates;

            controller.currentlySelectedUnit.attackState = CurrentActionState.inProgress;
            controller.GetGridReference().EnableCellIndicator(target, GridIndicatorMode.possibleDisarm);
            controller.StartCoroutine(ExecuteAttack(target, controller.currentlySelectedUnit));
            return true;
        }
        return false;
    }

    public override void CheckAdditionalCellIndicatorsConditions(IEnumerable<Vector3Int> indicatorsToCheck, GameGridManager grid, PlayerController controller)
    {
        List<Vector3Int> indicatorList = indicatorsToCheck.ToList();
        foreach (var mine in grid.GetEnemyMines(controller))
        {
            if (indicatorList.Contains(mine.coordinates))
            {
                CheckExplosionIndicators(mine.triggerTiles, GridIndicatorMode.explosionRangeNear, grid, controller);
                
                CheckExplosionIndicators(mine.detonationTiles, GridIndicatorMode.explosionRangeFar, grid, controller);

                grid.EnableCellIndicator(mine.coordinates, GridIndicatorMode.possibleDetonation, true);
            }
        }
    }

    void CheckExplosionIndicators(List<Vector3Int> coordinatesToCheck, GridIndicatorMode gridIndicatorMode, GameGridManager grid, PlayerController controller)
    {
        foreach (var coordinates in coordinatesToCheck)
        {
            Unit possibleUnit = grid.GetUnitAtCoordinates(coordinates);
            if (possibleUnit && possibleUnit.owner == controller)
            {
                continue;
            }
            grid.EnableCellIndicator(coordinates, gridIndicatorMode);
        }
    }

    public override float GetAttackMultiplier()
    {
        return centerDamageMultiplier;
    }
}
