using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Movement")] [System.Serializable]
public class ItemMovement : ItemData
{
    public override bool OnUse(Unit user)
    {
        if (user.possibleMovements.Count == 0)
        {
            user.ProcessRange(false);
        }
        return base.OnUse(user);
    }
}
