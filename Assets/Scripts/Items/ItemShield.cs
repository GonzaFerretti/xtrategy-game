using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Shield")]
public class ItemShield : ItemData
{
    public override bool OnUse(Unit user)
    {
        if (user.isShielded) return false;

        user.UpdateShieldStatus(true);

        return true;
    }
}
