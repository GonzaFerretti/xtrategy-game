using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/UnitAttributes")]
public class UnitAttributes : ScriptableObject
{
    public int maxHp;
    public int movementRange;
    public int minAttackRange;
    public int maxAttackRange;
    public int damage;
}
