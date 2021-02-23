using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : BaseController
{
    public Dictionary<int, AsyncAIActionResult> currentActions = new Dictionary<int, AsyncAIActionResult>();
    public Dictionary<Unit, AISavedData> AIUnitsSavedData = new Dictionary<Unit, AISavedData>();

    public override void StartTurn(bool shouldRestart = false)
    {
        base.StartTurn(shouldRestart);
        StartCoroutine(ExecuteUnitBehaviours());
    }

    IEnumerator ExecuteUnitBehaviours()
    {
        yield return new WaitForSeconds(1.5f);
        yield return CycleThroughUnitBehaviours();
        currentlySelectedUnit = null;
        gridManager.gameManager.EndPlayerTurn();

    }

    IEnumerator CycleThroughUnitBehaviours()
    {
        int startingUnitAmount = unitsControlled.Count;
        foreach (Unit unit in unitsControlled)
        {
            currentlySelectedUnit = unit;
            if (currentlySelectedUnit.HasActionsLeft())
            {
                Camera.main.GetComponent<CameraController>().SetFollowTarget(unit.transform);
                yield return StartCoroutine(unit.AI.ExecuteBehaviour(this, unit));
                if (unitsControlled.Count < startingUnitAmount)
                {
                    StartCoroutine(CycleThroughUnitBehaviours());
                    yield break;
                }
                yield return new WaitForSeconds(2);
            }
        }
    }


    IEnumerator InteractWithTarget(Unit interactedUnit, Unit interactingUnit, TargetType targetType)
    {
        interactingUnit.anim.Play("attack");
        interactingUnit.PlaySound(interactingUnit.unitAttributes.attackSound);
        interactingUnit.model.transform.forward = (interactedUnit.transform.position - interactingUnit.transform.position).normalized;
        yield return new WaitForSeconds(1);
        interactingUnit.anim.SetTrigger("endCurrentAnim");
        interactingUnit.unitAttributes.attackType.AttackAction(interactingUnit, interactedUnit);
        interactingUnit.attackState = CurrentActionState.ended;
    }

    public override void Update()
    {
        // Just do nothing on update :D
    }

    public bool WaitForAction()
    {
        return true;
    }

    public List<Unit> GetUnitsInAttackRange(List<Vector3Int> attacksInRange, TargetType targetType)
    {
        List<Unit> possibleUnitsToAttack = new List<Unit>();
        foreach (Unit unit in gridManager.gameManager.allUnits)
        {
            foreach (Vector3Int possibleAttackPosition in attacksInRange)
            {
                if (ShouldTargetUnit(unit,targetType))
                {
                    if (unit.GetCoordinates() == possibleAttackPosition)
                    {
                        possibleUnitsToAttack.Add(unit);
                        break;
                    }
                }
            }
        }
        return possibleUnitsToAttack;
    }

    bool ShouldTargetUnit(Unit unit, TargetType targetType)
    {
        return targetType == TargetType.AllUnits
            ||  (targetType == TargetType.AllyOnly && unitsControlled.Contains(unit))
            || (targetType == TargetType.EnemyOnly && !unitsControlled.Contains(unit));
    }

    public Unit GetLowestUnitInAttackRange(List<Vector3Int> attackRange, TargetType targetType)
    {
        List<Unit> unitsInAttackRange = GetUnitsInAttackRange(attackRange, targetType);
        if (unitsInAttackRange.Count > 0)
        {
            Unit lowestHpUnit = null;
            int lowestHP = int.MaxValue;
            foreach (Unit possibleUnitToAttack in unitsInAttackRange)
            {
                if (possibleUnitToAttack.currentHp < lowestHP)
                {
                    lowestHP = possibleUnitToAttack.currentHp;
                    lowestHpUnit = possibleUnitToAttack;
                }
            }
            return lowestHpUnit;
        }
        return null;
    }

    public List<Unit> GetUnitsFromOthers()
    {
        List<Unit> otherUnits = new List<Unit>();

        foreach (Unit possibleUnit in gridManager.gameManager.allUnits)
        {
            if (!unitsControlled.Contains(possibleUnit))
            {
                otherUnits.Add(possibleUnit);
            }
        }
        return otherUnits;
    }

    public void EndActionResult(AsyncAIActionResult actionResult)
    {
        currentActions.Remove(actionResult.id);
    }

    public AsyncAIActionResult GenerateNewAIActionResult()
    {
        int queryId = Random.Range(0, 100);
        while (currentActions.ContainsKey(queryId))
        {
            queryId = Random.Range(0, 100);
        }

        AsyncAIActionResult actionResult = new AsyncAIActionResult(queryId, this);
        currentActions.Add(queryId, actionResult);
        return actionResult;
    }

    public IEnumerator AttemptInteractWithLowestHPTarget(Unit actingUnit, int id, TargetType targetType = TargetType.EnemyOnly)
    {
        AsyncRangeQuery attackQuery = actingUnit.StartAttackRangeQuery();

        if (currentlySelectedUnit.attackState == CurrentActionState.ended)
        {
            currentActions[id].endedSuccesfully = false;
            yield break;
        }

        currentlySelectedUnit.attackState = CurrentActionState.inProgress;

        while (!attackQuery.hasFinished)
        {
            yield return null;
        }
        if (attackQuery.cellsInRange.Count > 0)
        {
            Unit lowestHpUnitInRange = GetLowestUnitInAttackRange(attackQuery.cellsInRange, targetType);
            if (lowestHpUnitInRange)
            {
                yield return StartCoroutine(InteractWithTarget(lowestHpUnitInRange, actingUnit, targetType));
                currentActions[id].endedSuccesfully = true;
            }
            else
            {
                currentActions[id].endedSuccesfully = false;
            }
        }
        else
        {
            currentActions[id].endedSuccesfully = false;
        }

        attackQuery.End();
    }

    public IEnumerator AttemptToDetonateMine(Unit actingUnit, int id)
    {
        AsyncRangeQuery attackQuery = actingUnit.StartAttackRangeQuery();

        if (currentlySelectedUnit.attackState == CurrentActionState.ended)
        {
            currentActions[id].endedSuccesfully = false;
            yield break;
        }

        currentlySelectedUnit.attackState = CurrentActionState.inProgress;

        while (!attackQuery.hasFinished)
        {
            yield return null;
        }
        if (attackQuery.cellsInRange.Count > 0)
        {
            MagicMine unitInRangeOfMine = gridManager.GetMineWithEnemyNearby(attackQuery.cellsInRange, actingUnit.owner);
            if (unitInRangeOfMine)
            {
                gridManager.DetonateMine(unitInRangeOfMine.coordinates,actingUnit);
                currentActions[id].endedSuccesfully = true;
            }
            else
            {
                currentActions[id].endedSuccesfully = false;
            }
        }
        else
        {
            currentActions[id].endedSuccesfully = false;
        }

        attackQuery.End();
    }

    public IEnumerator MoveTowardsClosestTarget(Unit actingUnit, List<Unit> unitsToInteract)
    {
        if (!currentlySelectedUnit) yield break;
        if (currentlySelectedUnit.moveState == CurrentActionState.ended)
        {
            yield break;
        }

        GameGridManager grid = GetGridReference();
        AsyncPathQuery query = grid.StartBestPathToClosestUnitQuery(actingUnit, unitsToInteract);
        currentlySelectedUnit.moveState = CurrentActionState.inProgress;
        while (!query.hasFinished)
        {
            yield return null;
        }
        Vector3Int[] pathToClosestTarget = query.GetPathArray();
        if (pathToClosestTarget.Length > 0)
        {
            int movementRange = actingUnit.movementRange;
            if (pathToClosestTarget.Length > movementRange)
            {
                Vector3Int[] longestPossiblePath = new Vector3Int[movementRange];
                for (int i = 0; i < movementRange; i++)
                {
                    longestPossiblePath[i] = pathToClosestTarget[i];
                }
                pathToClosestTarget = longestPossiblePath;
            }
            yield return StartCoroutine(actingUnit.MoveAlongPath(pathToClosestTarget));
        }
    }

    public IEnumerator MoveTowardsClosestMine(Unit actingUnit)
    {
        if (!currentlySelectedUnit) yield break;
        if (currentlySelectedUnit.moveState == CurrentActionState.ended)
        {
            yield break;
        }

        GameGridManager grid = GetGridReference();
        AsyncPathQuery query = grid.StartBestPathToClosestMineQuery(actingUnit);
        currentlySelectedUnit.moveState = CurrentActionState.inProgress;
        while (!query.hasFinished)
        {
            yield return null;
        }
        Vector3Int[] pathToClosestMine = query.GetPathArray();
        if (pathToClosestMine.Length > 0)
        {
            int movementRange = actingUnit.movementRange;
            if (pathToClosestMine.Length > movementRange)
            {
                Vector3Int[] longestPossiblePath = new Vector3Int[movementRange];
                for (int i = 0; i < movementRange; i++)
                {
                    longestPossiblePath[i] = pathToClosestMine[i];
                }
                pathToClosestMine = longestPossiblePath;
            }
            yield return StartCoroutine(actingUnit.MoveAlongPath(pathToClosestMine));
        }
    }

    public IEnumerator MoveTowardsCoverCloseToEnemy(Unit actingUnit, int id)
    {
        if (!currentlySelectedUnit)
        {
            currentActions[id].endedSuccesfully = false;
            yield break;
        }

        if (currentlySelectedUnit.moveState == CurrentActionState.ended)
        {
            yield break;
        }
        currentActions[id].endedSuccesfully = false;

        AsyncPathQuery query = GetGridReference().StartPathCoverClosestToEnemyQuery(actingUnit, GetUnitsFromOthers());
        currentlySelectedUnit.moveState = CurrentActionState.inProgress;
        while (!query.hasFinished)
        {
            yield return null;
        }

        Vector3Int[] pathToClosestEnemy = query.GetPathArray();

        query.End();

        if (pathToClosestEnemy.Length > 0)
        {
            int movementRange = actingUnit.movementRange;
            if (pathToClosestEnemy.Length > movementRange)
            {
                Vector3Int[] longestPossiblePath = new Vector3Int[movementRange];
                for (int i = 0; i < movementRange; i++)
                {
                    longestPossiblePath[i] = pathToClosestEnemy[i];
                }
                pathToClosestEnemy = longestPossiblePath;
            }
            currentActions[id].endedSuccesfully = true;
            yield return StartCoroutine(actingUnit.MoveAlongPath(pathToClosestEnemy));
        }
        else
        {
            currentActions[id].endedSuccesfully = false;
        }
    }
}

public enum TargetType
{
    AllyOnly,
    EnemyOnly,
    AllUnits
}

public struct AISavedData 
{
    public int lastAttackTurn;
}