using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackType : ScriptableObject
{
    [SerializeField] public bool shouldAllowAllyTargeting;

    public virtual float GetAttackMultiplier() { return 1f; } 

    public abstract bool CheckPossibleTarget(PlayerController controller);

    public abstract IEnumerator ExecuteAttack(Vector3Int coordinatesToAttack, Unit attackingUnit);

    public virtual void AttackAction(Unit attackingUnit, Unit attackedUnit) { }

    public abstract void CheckAdditionalCellIndicatorsConditions(IEnumerable<Vector3Int> indicatorsToCheck, GameGridManager grid, PlayerController controller);

    public virtual int CalculateFinalDamage(Unit attackingUnit, bool shouldUseMultiplier)
    {
        float multiplier = shouldUseMultiplier ? GetAttackMultiplier() : 1;
        return Mathf.RoundToInt((attackingUnit.damage + (attackingUnit.HasBuff("attackBoost") ? attackingUnit.attributes.mainAttack.damageBoost : 0))*multiplier);
    }
}
