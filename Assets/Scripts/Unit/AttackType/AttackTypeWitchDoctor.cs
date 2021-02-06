using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/Attack Types/Witch Doctor")]
public class AttackTypeWitchDoctor : AttackType
{
    [SerializeField] float healMultiplier;

    [SerializeField] int turns;

    public override IEnumerator ExecuteAttack(Vector3Int coordinatesToAttack, Unit attackingUnit)
    {
        Unit unitToInteract = attackingUnit.owner.GetGridReference().GetUnitAtCoordinates(coordinatesToAttack);

        attackingUnit.anim.Play("attack");
        attackingUnit.PlaySound(attackingUnit.unitAttributes.attackSound);
        attackingUnit.model.transform.forward = (unitToInteract.transform.position - attackingUnit.transform.position).normalized;
        yield return new WaitForSeconds(1);
        attackingUnit.anim.SetTrigger("endCurrentAnim");
        attackingUnit.TryConsumeBuff("attackBoost");
        if (unitToInteract.owner != attackingUnit.owner)
        {
            unitToInteract.TakeDamage(CalculateFinalDamage(attackingUnit), attackingUnit.GetCoordinates(), true);
            Buff poisonBuff = attackingUnit.grid.gameManager.saveManager.buffTypeBank.GetBuffType("poison");
            poisonBuff = Instantiate(poisonBuff);
            poisonBuff.charges = turns;
            unitToInteract.TryAddBuff(poisonBuff, false);
        }
        else if (unitToInteract.IsDamaged())
            unitToInteract.Heal(Mathf.RoundToInt(CalculateFinalDamage(attackingUnit) * healMultiplier));

        attackingUnit.attackState = CurrentActionState.ended;
    }

    public override bool CheckPossibleTarget(PlayerController controller)
    {
        controller.CheckUnitUISwitch();
        if (controller.GetObjectUnderMouse(out GameObject objectSelected, 1 << LayerMask.NameToLayer("Unit")))
        {
            Unit unitSelected = objectSelected.GetComponent<Unit>();
            Vector3Int unitPosition = unitSelected.GetCoordinates();
            if (controller.currentlySelectedUnit.possibleAttacks.Contains(unitPosition))
            {
                if (unitSelected.owner == controller && !unitSelected.IsDamaged()) return false;
                controller.currentlySelectedUnit.attackState = CurrentActionState.inProgress;
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
                    grid.EnableCellIndicator(coordinates, GridIndicatorMode.possibleAttack);
                }
                else
                {
                    if (unit.IsDamaged())
                        grid.EnableCellIndicator(coordinates, GridIndicatorMode.possibleHeal);
                    else grid.DisableCellIndicator(coordinates);
                }
            }
        }
    }
}
