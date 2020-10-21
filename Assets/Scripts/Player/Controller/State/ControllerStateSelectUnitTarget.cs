using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu(menuName = "Controller/State/Select Attack Target")]
public class ControllerStateSelectUnitTarget : ControllerState
{
    public override void OnUpdate()
    {
        base.OnUpdate();
        if (Input.GetMouseButtonDown(0) && !CheckPossibleAttackTarget()) CheckUnitDeselect();
        if (controller.currentlySelectedUnit) (controller as PlayerController).OnHoverGrid(GridIndicatorMode.possibleAttack, GridIndicatorMode.selectedAttack, controller.currentlySelectedUnit.possibleAttacks);
    }

    bool CheckUnitDeselect()
    {
        if ((controller as PlayerController).GetObjectUnderMouse(out GameObject objectSelected, 1 << LayerMask.NameToLayer("Unit")))
        {
            Unit unitSelected = objectSelected.GetComponent<Unit>();
            if (!controller.OwnsUnit(unitSelected)) return false;
            controller.currentlySelectedUnit.Deselect();
            controller.currentlySelectedUnit = unitSelected;
            controller.currentlySelectedUnit.Select();
            return true;
        }
        else
        {
            if (!EventSystem.current.currentSelectedGameObject)
            {
                controller.currentlySelectedUnit.Deselect();
                controller.currentlySelectedUnit = null;
                return true;
            }
        }
        return false;
    }

    bool CheckPossibleAttackTarget()
    {
        if ((controller as PlayerController).GetObjectUnderMouse(out GameObject objectSelected, 1 << LayerMask.NameToLayer("Unit")))
        {
            Unit unitSelected = objectSelected.GetComponent<Unit>();
            if (unitSelected == controller.currentlySelectedUnit) return false;
            Vector3Int unitPosition = unitSelected.GetCoordinates();
            if (controller.currentlySelectedUnit.possibleAttacks.Contains(unitPosition))
            {
                controller.currentlySelectedUnit.attackState = currentActionState.inProgress;
                controller.StartCoroutine(AttackEnemy(unitSelected));
                controller.GetGridReference().DisableAllCellIndicators();
                return true;
            }
        }
        return false;
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
