using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackType : ScriptableObject
{
    public abstract bool CheckPossibleTarget(PlayerController controller);

    public abstract IEnumerator ExecuteAttack(Vector3Int coordinatesToAttack, Unit attackingUnit);

    public abstract void CheckAdditionalCellIndicatorsConditions(IEnumerable<Vector3Int> indicatorsToCheck, GameGridManager grid, PlayerController controller);
}
