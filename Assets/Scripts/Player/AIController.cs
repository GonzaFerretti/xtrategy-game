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
}
