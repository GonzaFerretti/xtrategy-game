using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackType : ScriptableObject
{
    [SerializeField] public bool shouldAllowAllyTargeting;

    public virtual float GetAttackMultiplier() { return 1f; } 

    public abstract bool CheckPossibleTarget(PlayerController controller);

    public abstract IEnumerator ExecuteAttack(Vector3Int coordinatesToAttack, Unit attackingUnit);

    public virtual void AttackAction(Unit attackingUnit, Unit attackedUnit, AttackAttributes attributesToUse = null) { }

    public abstract void CheckAdditionalCellIndicatorsConditions(IEnumerable<Vector3Int> indicatorsToCheck, GameGridManager grid, PlayerController controller);

    public virtual int CalculateFinalDamage(Unit attackingUnit, bool shouldUseMultiplier, out DamageType damageType, AttackAttributes attributesToUse = null)
    {
        if (!attributesToUse)
        {
            attributesToUse = attackingUnit.attributes.mainAttack;
        }
        float multiplier = shouldUseMultiplier ? GetAttackMultiplier() : 1;

        bool isBoosted = attackingUnit.HasBuff("attackBoost");

        damageType = isBoosted ? DamageType.boosted : DamageType.normal;

        return Mathf.RoundToInt((attributesToUse.damage + (isBoosted ? attributesToUse.damageBoost : 0))*multiplier);
    }
}
