using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/Attack Types/Suicider")]
public class AttackTypeSuicider : AttackTypeDirect
{
    [SerializeField] int explosionRange;
    [SerializeField] int centerRange;

    public override void AttackAction(Unit attackingUnit, Unit attackedUnit, AttackAttributes attributesToUse = null)
    {
        Vector3Int center = attackingUnit.GetCoordinates();
        GameGridManager grid = attackingUnit.grid;

        // Little reuse for the tank boss because I'm lazy
        bool shouldDoSuicideBomb = attributesToUse == attackingUnit.attributes.mainAttack;
        grid.CreateVisualExplosion(center);

        for (int x = center.x - explosionRange; x <= center.x + explosionRange; x++)
        {
            for (int y = center.y - explosionRange; y <= center.y + explosionRange; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                GameGridCell cell;

                if (cell = grid.GetCellAtCoordinate(pos))
                {
                    bool shouldDoCenterExplosion = !shouldDoSuicideBomb && (Mathf.Abs(x) <= centerRange && Mathf.Abs(y) <= centerRange);
                    grid.TriggerExplosion(pos, center, shouldDoCenterExplosion, attackingUnit);
                }
                else continue;
            }
        }

        if (shouldDoSuicideBomb)
        {
            attackingUnit.TakeDamage(99999, Vector3Int.zero, false, DamageType.boosted);
        }
    }
}
