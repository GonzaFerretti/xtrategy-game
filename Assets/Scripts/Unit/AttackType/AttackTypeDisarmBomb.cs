using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/Attack Types/Disarm bomb")]
public class AttackTypeDisarmBomb : AttackType
{
    public override IEnumerator ExecuteAttack(Vector3Int coordinatesToAttack, Unit attackingUnit)
    {
        attackingUnit.anim.Play("attack");
        attackingUnit.PlaySound(attackingUnit.unitAttributes.attackSound);

        yield return new WaitForSeconds(1);
        attackingUnit.anim.SetTrigger("endCurrentAnim");
        attackingUnit.grid.DetonateMine(coordinatesToAttack, attackingUnit.owner);
        attackingUnit.attackState = CurrentActionState.ended;
    }

    public override bool CheckPossibleTarget(PlayerController controller)
    {
        controller.CheckUnitUISwitch();

        if (controller.GetObjectUnderMouse(out GameObject objectSelected, 1 << LayerMask.NameToLayer("Mine")))
        {
            Vector3Int target = objectSelected.GetComponent<MagicMine>().coordinates;

            controller.currentlySelectedUnit.attackState = CurrentActionState.inProgress;
            controller.StartCoroutine(ExecuteAttack(target, controller.currentlySelectedUnit));
            return true;
        }
        return false;
    }

    public override void CheckAdditionalCellIndicatorsConditions(IEnumerable<Vector3Int> indicatorsToCheck, GameGridManager grid, PlayerController controller)
    {
        foreach (var coordinates in indicatorsToCheck)
        {
            if (grid.mineTriggerTiles.ContainsKey(coordinates))
            {
                var mine = grid.mineTriggerTiles[coordinates];
                if (mine.owner != controller)
                {
                    grid.EnableCellIndicator(coordinates, GridIndicatorMode.possibleDisarm);
                }
            }
        }
    }
}
