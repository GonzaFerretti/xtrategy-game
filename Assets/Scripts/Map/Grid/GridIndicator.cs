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
        rend.material = availableMaterials.GetMaterial(mode);
    }
}

public enum GridIndicatorMode
{
    disabled,
    possibleMovement,
    possibleAttack,
    selectedAttack,
    selectedMovement,
}
