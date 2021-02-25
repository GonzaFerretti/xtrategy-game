using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/AiBehaviour/TankBoss")]
public class AIBehaviourTankBoss : AIBehaviour
{
    

    public override IEnumerator ExecuteBehaviour(AIController controller, Unit unit)
    {
        yield return null;
    }
}

[System.Serializable]
public struct BossAttackConfig
{
    int range;
    int attackDamage;
    int boostedAttack;
}