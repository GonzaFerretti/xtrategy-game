using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : BaseController
{
    public Dictionary<int, AsyncAIActionResult> currentActions = new Dictionary<int, AsyncAIActionResult>();
    public override void StartTurn()
    {
        base.StartTurn();
        StartCoroutine(ExecuteUnitBehaviours());
    }

    IEnumerator ExecuteUnitBehaviours()
    {
        foreach (Unit unit in unitsControlled)
        {
            yield return StartCoroutine(unit.AI.ExecuteBehaviour(this, unit));
        }
        gridManager.gameManager.EndPlayerTurn();

    }

    public void Attack(Unit attackedUnit, Unit attackingUnit)
    {
        attackedUnit.Damage(attackingUnit.damage);
        attackingUnit.attackState = currentActionState.ended;
    }

    public override void Update()
    {

    }

    public bool WaitForAction()
    {
        return true;
    }

    public List<Unit> GetUnitsInAttackRange(List<Vector3Int> attacksInRange)
    {
        List<Unit> possibleUnitsToAttack = new List<Unit>();
        foreach (Unit unit in gridManager.gameManager.allUnits)
        {
            foreach (Vector3Int possibleAttackPosition in attacksInRange)
            {
                if (!unitsControlled.Contains(unit))
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

    public Unit GetLowestUnitInAttackRange(List<Vector3Int> attackRange)
    {
        List<Unit> unitsInAttackRange = GetUnitsInAttackRange(attackRange);
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



    public IEnumerator AttemptAttack(Unit actingUnit, int id)
    {
        AsyncRangeQuery attackQuery = actingUnit.StartAttackRangeQuery();

        while (!attackQuery.hasFinished)
        {
            yield return null;
        }
        if (attackQuery.cellsInRange.Count > 0)
        {
            Unit lowestHpUnitInRange = GetLowestUnitInAttackRange(attackQuery.cellsInRange);
            if (lowestHpUnitInRange)
            {
                Attack(lowestHpUnitInRange, actingUnit);
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

        attackQuery.EndQuery();
    }

    public IEnumerator MoveTowardsClosestEnemy(Unit actingUnit)
    {
        Vector3Int[] pathToClosestEnemy = GetGridReference().GetBestPathToGetToClosestUnit(actingUnit, GetUnitsFromOthers());
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
            yield return StartCoroutine(actingUnit.MoveAlongPath(pathToClosestEnemy));
        }
    }

    public IEnumerator MoveTowardsCoverCloseToEnemy(Unit actingUnit, int id)
    {
        Vector3Int[] pathToClosestEnemy = GetGridReference().GetPathToCoverClosestToEnemy(actingUnit, GetUnitsFromOthers());
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
