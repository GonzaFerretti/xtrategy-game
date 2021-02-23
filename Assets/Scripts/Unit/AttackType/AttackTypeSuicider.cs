using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/Attack Types/Suicider")]
public class AttackTypeSuicider : AttackTypeDirect
{
    [SerializeField] int explosionRange;

    public override void AttackAction(Unit attackingUnit, Unit attackedUnit)
    {
        Vector3Int center = attackingUnit.GetCoordinates();
        GameGridManager grid = attackingUnit.grid;

        for (int x = center.x - explosionRange; x <= center.x + explosionRange; x++)
        {
            for (int y = center.y - explosionRange; y <= center.y + explosionRange; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                GameGridCell cell;

                if (cell = grid.GetCellAtCoordinate(pos))
                {
                    grid.TriggerExplosion(pos, center, false, attackingUnit);
                }
                else continue;
            }
        }

        attackingUnit.TakeDamage(99999, Vector3Int.zero, false);
    }
}
