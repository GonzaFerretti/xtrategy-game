﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller/State/Select Attack Target")]
public class ControllerStateSelectUnitTarget : ControllerState
{
    public override void OnUpdate()
    {
        base.OnUpdate();
        if (Input.GetMouseButtonDown(0) && !CheckAttackAction()) (controller as PlayerController).CheckUnitDeselect();
        if (controller.currentlySelectedUnit) (controller as PlayerController).OnHoverGrid(GridIndicatorMode.attackRange, GridIndicatorMode.possibleAttack, controller.currentlySelectedUnit.possibleAttacks);
    }

    public override void OnTransitionIn()
    {
        base.OnTransitionIn();
        controller.StartCoroutine(WaitForAttackListReady());
        controller.GetGridReference().EnableCellIndicator(controller.currentlySelectedUnit.GetCoordinates(), GridIndicatorMode.selectedUnit);
    }

    bool CheckAttackAction()
    {
        if (controller.currentlySelectedUnit == null) return false;
        return controller.currentlySelectedUnit.attributes.mainAttack.attackType.CheckPossibleTarget(controller as PlayerController);
    }

    IEnumerator WaitForAttackListReady()
    {
        float startTime = Time.time;
        while (controller.currentlySelectedUnit && controller.currentlySelectedUnit.possibleAttacks.Count == 0)
        {
            // TO DO: is this really necessary now?
            if (Time.time - startTime > 2f) 
            {
                yield break;
            }
            yield return null;
        }

        if (!controller.currentlySelectedUnit) yield break;

        GameGridManager gm = controller.GetGridReference();
        gm.SetCoverIndicators(controller.currentlySelectedUnit.possibleAttacks, true);
    }

    public override void OnTransitionOut()
    {
        controller.GetGridReference().SetAllCoverIndicators(false);
        controller.GetGridReference().DisableAllCellIndicators();
    }
}
