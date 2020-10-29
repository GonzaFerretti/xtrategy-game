using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/State/Select Attack Target")]
public class ControllerStateSelectUnitTarget : ControllerState
{
    public override void OnUpdate()
    {
        base.OnUpdate();
        if (Input.GetMouseButtonDown(0) && !CheckPossibleAttackTarget()) (controller as PlayerController).CheckUnitDeselect();
        if (controller.currentlySelectedUnit) (controller as PlayerController).OnHoverGrid(GridIndicatorMode.possibleAttack, GridIndicatorMode.selectedAttack, controller.currentlySelectedUnit.possibleAttacks);
    }

    public override void OnTransitionIn()
    {
        controller.StartCoroutine(WaitForAttackListReady());
        controller.GetGridReference().EnableCellIndicator(controller.currentlySelectedUnit.GetCoordinates(), GridIndicatorMode.selectedUnit);
    }

    IEnumerator WaitForAttackListReady()
    {
        float startTime = Time.time;
        while (controller.currentlySelectedUnit.possibleAttacks.Count == 0)
        {
            if (Time.time - startTime > 2f) 
            {
                yield break;
            }
            yield return null;
        }

        GameGridManager gm = controller.GetGridReference();
        gm.SetCoverIndicators(controller.currentlySelectedUnit.possibleAttacks, true);
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
                controller.currentlySelectedUnit.attackState = CurrentActionState.inProgress;
                controller.StartCoroutine(AttackEnemy(unitSelected));
                return true;
            }
        }
        return false;
    }

    public override void OnTransitionOut()
    {
        controller.GetGridReference().SetAllCoverIndicators(false);
        controller.GetGridReference().DisableAllCellIndicators();
    }

    IEnumerator AttackEnemy(Unit enemyToAttack)
    {
        controller.currentlySelectedUnit.anim.Play("attack");
        controller.currentlySelectedUnit.PlaySound(controller.currentlySelectedUnit.unitAttributes.attackSound);
        controller.currentlySelectedUnit.model.transform.forward = (enemyToAttack.transform.position - controller.currentlySelectedUnit.transform.position).normalized;
        yield return new WaitForSeconds(1);
        controller.currentlySelectedUnit.anim.SetTrigger("endCurrentAnim");
        enemyToAttack.TakeDamage(controller.currentlySelectedUnit.damage, controller.currentlySelectedUnit);
        controller.currentlySelectedUnit.attackState = CurrentActionState.ended;
    }
}
