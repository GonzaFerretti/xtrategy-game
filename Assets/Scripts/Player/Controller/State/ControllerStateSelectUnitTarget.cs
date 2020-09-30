using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/State/Select Attack Target")]
public class ControllerStateSelectUnitTarget : ControllerState
{
    public override void OnUpdate()
    {
        base.OnUpdate();
        if (Input.GetMouseButtonDown(1)) CheckPossibleAttackTarget();
        if (Input.GetMouseButtonDown(0)) CheckUnitDeselect();
        if (controller.currentlySelectedUnit) (controller as PlayerController).OnHoverGrid(GridIndicatorMode.possibleAttack, GridIndicatorMode.selectedAttack, controller.currentlySelectedUnit.possibleAttacks);
    }

    public void CheckUnitDeselect()
    {
        GameObject objectSelected;
        controller.currentlySelectedUnit.Deselect();

        if ((controller as PlayerController).GetObjectUnderMouse(out objectSelected, 1 << LayerMask.NameToLayer("Unit")))
        {
            Unit unitSelected = objectSelected.GetComponent<Unit>();
            if (!controller.OwnsUnit(unitSelected)) return;
            controller.currentlySelectedUnit = unitSelected;
            controller.currentlySelectedUnit.Select();
        }
        else
        {
            controller.currentlySelectedUnit = null;
        }
    }

    public void CheckPossibleAttackTarget()
    {
        GameObject objectSelected;
        if ((controller as PlayerController).GetObjectUnderMouse(out objectSelected, 1 << LayerMask.NameToLayer("Unit")))
        {
            Unit unitSelected = objectSelected.GetComponent<Unit>();
            if (unitSelected == controller.currentlySelectedUnit) return;
            Vector3Int unitPosition = unitSelected.GetCoordinates();
            if (controller.currentlySelectedUnit.possibleAttacks.Contains(unitPosition))
            {
                controller.currentlySelectedUnit.attackState = currentActionState.inProgress;
                controller.StartCoroutine(AttackEnemy(unitSelected));
                controller.GetGridReference().DisableAllCellIndicators();
            }
        }
    }

    IEnumerator AttackEnemy(Unit enemyToAttack)
    {
        controller.currentlySelectedUnit.anim.Play("attack");
        controller.currentlySelectedUnit.PlaySound(controller.currentlySelectedUnit.unitAttributes.attackSound);
        controller.currentlySelectedUnit.model.transform.forward = (enemyToAttack.transform.position - controller.currentlySelectedUnit.transform.position).normalized;
        yield return new WaitForSeconds(1);
        controller.currentlySelectedUnit.anim.SetTrigger("endCurrentAnim");
        enemyToAttack.TakeDamage(controller.currentlySelectedUnit.damage, controller.currentlySelectedUnit);
        controller.currentlySelectedUnit.attackState = currentActionState.ended;
    }
}
