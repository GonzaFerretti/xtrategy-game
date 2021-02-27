using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridIndicator : MonoBehaviour
{
    [SerializeField] GridIndicatorMaterials availableMaterials;
    bool isLocked = false;
    [SerializeField] Renderer rend;

    public void Disable(bool forceUnlock = false)
    {
        if (!isLocked || forceUnlock)
        {
            rend.material = availableMaterials.GetMaterial(GridIndicatorMode.disabled);
            isLocked = false;
        }
    }

    public void Enable(GridIndicatorMode mode, bool shouldLock = false)
    {
        Material material = availableMaterials.GetMaterial(mode);
        if (material && !isLocked)
        {
            rend.material = material;
            isLocked = shouldLock;
        }
    }
}

public enum GridIndicatorMode
{
    disabled,
    movementRange,
    attackRange,
    possibleAttack,
    selectedMovement,
    possibleMine,
    possibleDisarm,
    possibleHeal,
    selectedUnit,
    item,
    possibleDetonation,
    explosionRangeNear,
    explosionRangeFar,
    poison,
}
