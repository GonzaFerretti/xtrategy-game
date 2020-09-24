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
                controller.GetGridReference().UntintAll();
            }
        }
    }

    IEnumerator AttackEnemy(Unit enemyToAttack)
    {
        yield return new WaitForSeconds(1);
        enemyToAttack.Damage(controller.currentlySelectedUnit.damage);
        controller.currentlySelectedUnit.attackState = currentActionState.ended;
    }
}
