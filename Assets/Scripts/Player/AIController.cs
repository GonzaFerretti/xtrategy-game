using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : BaseController
{
    public override void Start()
    {
        base.Start();
    }

    IEnumerator CalculateMovementsToCover()
    {
        yield return new WaitForEndOfFrame();
        List<AsyncRangeQuery> RangeQueries; 
        foreach (Unit unit in unitsControlled)
        {
            //AsyncRangeQuery currentRangeQuery = gridManager.QueryUnitAttackRange(minAttackRange, maxAttackRange, currentCellCoords);
            //possibleAttacks = new List<Vector3Int>();
        }
        
    }
}
