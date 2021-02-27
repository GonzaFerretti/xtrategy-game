using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/BossAttributes")] [System.Serializable]
public class BossAttributes : UnitAttributes
{
    public AttackAttributes secondAttack;
    public AttackAttributes thirdAttack;
}
