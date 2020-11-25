using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/Attack Types/Direct")]
public class AttackTypeDirect : AttackType
{
    public override IEnumerator ExecuteAttack(Vector3Int coordinatesToAttack, Unit attackingUnit)
    {
        Unit enemyToAttack = attackingUnit.owner.GetGridReference().GetUnitAtCoordinates(coordinatesToAttack);

        attackingUnit.anim.Play("attack");
        attackingUnit.PlaySound(attackingUnit.unitAttributes.attackSound);
        attackingUnit.model.transform.forward = (enemyToAttack.transform.position - attackingUnit.transform.position).normalized;
        yield return new WaitForSeconds(1);
        attackingUnit.anim.SetTrigger("endCurrentAnim");
        enemyToAttack.TakeDamage(attackingUnit.damage, attackingUnit.GetCoordinates());
        attackingUnit.attackState = CurrentActionState.ended;
    }

    public override bool CheckPossibleTarget(PlayerController controller)
    {
        controller.CheckUnitUISwitch();
        if (controller.GetObjectUnderMouse(out GameObject objectSelected, 1 << LayerMask.NameToLayer("Unit")))
        {
            Unit unitSelected = objectSelected.GetComponent<Unit>();
            if (unitSelected == controller.currentlySelectedUnit) return false;
            Vector3Int unitPosition = unitSelected.GetCoordinates();
            if (controller.currentlySelectedUnit.possibleAttacks.Contains(unitPosition))
            {
                controller.currentlySelectedUnit.attackState = CurrentActionState.inProgress;
                controller.StartCoroutine(ExecuteAttack(unitPosition,controller.currentlySelectedUnit));
                return true;
            }
        }
        return false;
    }
}
