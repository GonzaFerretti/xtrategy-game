using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/AiBehaviour/Witch Doctor")]
public class AIBehaviourWitchDoctor : AIBehaviour
{
    public override IEnumerator ExecuteBehaviour(AIController controller, Unit actingUnit)
    {
        yield return null;
        /*
        if (allyWounded)
        {
            if (allyClose)
            {
                if (TryHeal())
                {
                    if (allyWounded)
                    {
                        TryMoveToAlly()
                    }
                    else
                    {
                        TryMoveToEnemy()
                    }
                }
                // If it's close, it won't fail.
            }
            else
            {
                if (TryMoveToAlly())
                {
                    if (Allyclose)
                    {
                        TryHeal();
                    }
                    else
                    {
                        TryAttack();
                    }
                }
                // Can you even not move towards someone?
                else
                {
                    if (TryAttack())
                    {
                        TryMoveToClosestCover();
                    }
                }
            }
        }
        else
        {
            if (TryAttack())
            {
                TryMoveToClosestCover();
            }
            else
            {
                TryMoveToEnemy();
                TryAttack();
            }
        }
        */
    }
}