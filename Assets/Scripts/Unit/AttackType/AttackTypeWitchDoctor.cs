﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/Attack Types/Witch Doctor")]
public class AttackTypeWitchDoctor : AttackType
{
    [SerializeField] float healMultiplier;

    [SerializeField] SoundClip healSound;

    [SerializeField] int turns;

    public override IEnumerator ExecuteAttack(Vector3Int coordinatesToAttack, Unit attackingUnit)
    {
        Unit unitToInteract = attackingUnit.owner.GetGridReference().GetUnitAtCoordinates(coordinatesToAttack);

        attackingUnit.model.transform.forward = (unitToInteract.transform.position - attackingUnit.transform.position).normalized;
        yield return new WaitForSeconds(1);
        attackingUnit.anim.SetTrigger("endCurrentAnim");
        attackingUnit.TryConsumeBuff("attackBoost");
        AttackAction(attackingUnit, unitToInteract);

        attackingUnit.attackState = CurrentActionState.ended;
    }

    public override void AttackAction(Unit attackingUnit, Unit attackedUnit, AttackAttributes attributesToUse = null)
    {
        if (attackedUnit.owner != attackingUnit.owner)
        {
            attackedUnit.TakeDamage(CalculateFinalDamage(attackingUnit, false, out DamageType type, attributesToUse), attackingUnit.GetCoordinates(), true, type);

            attackingUnit.anim.Play("Attack");

            attackingUnit.PlaySound(attackingUnit.attributes.mainAttack.attackSound);
            Buff poisonBuff = attackingUnit.grid.gameManager.saveManager.buffTypeBank.GetBuffType("poison");
            poisonBuff = Instantiate(poisonBuff);
            poisonBuff.charges = turns;
            attackedUnit.TryAddBuff(poisonBuff, false);
        }
        else if (attackedUnit.IsDamaged())
        {
            attackedUnit.Heal(CalculateFinalDamage(attackingUnit, true, out DamageType type, attributesToUse));
            attackingUnit.anim.Play("Heal");
            attackingUnit.PlaySound(healSound);
        }
    }

    public override bool CheckPossibleTarget(PlayerController controller)
    {
        controller.CheckUnitUISwitch();
        if (controller.GetObjectUnderMouse(out Unit unitSelected, 1 << LayerMask.NameToLayer("Unit")))
        {
            Vector3Int unitPosition = unitSelected.GetCoordinates();
            if (controller.currentlySelectedUnit.possibleAttacks.Contains(unitPosition))
            {
                bool isAllyUnit = unitSelected.owner == controller;
                if (isAllyUnit && !unitSelected.IsDamaged()) return false;
                controller.currentlySelectedUnit.attackState = CurrentActionState.inProgress;
                controller.GetGridReference().EnableCellIndicator(unitPosition, isAllyUnit ? GridIndicatorMode.possibleHeal : GridIndicatorMode.poison);
                controller.StartCoroutine(ExecuteAttack(unitPosition, controller.currentlySelectedUnit));
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
            if (unit)
            {
                if (unit.owner != controller)
                {
                    grid.EnableCellIndicator(coordinates, GridIndicatorMode.poison, true);
                }
                else
                {
                    if (unit.IsDamaged())
                        grid.EnableCellIndicator(coordinates, GridIndicatorMode.possibleHeal, true);
                    else grid.DisableCellIndicator(coordinates);
                }
            }
        }
    }

    public override float GetAttackMultiplier()
    {
        return healMultiplier;
    }
}
