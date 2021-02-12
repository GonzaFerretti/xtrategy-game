using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridIndicator : MonoBehaviour
{
    [SerializeField] GridIndicatorMaterials availableMaterials;

    [SerializeField] Renderer rend;

    public void Disable()
    {
        rend.material = availableMaterials.GetMaterial(GridIndicatorMode.disabled);
    }

    public void Enable(GridIndicatorMode mode)
    {
        Material material = availableMaterials.GetMaterial(mode);
        if (material)
        {
            rend.material = material;
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
