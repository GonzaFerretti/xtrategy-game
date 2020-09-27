using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/Grid Indicators Materials")]
public class GridIndicatorMaterials : ScriptableObject
{
    [SerializeField] List<Material> materials;

    public Material GetMaterial(GridIndicatorMode mode)
    {
        Material mat = null;
        foreach (Material possibleMat in materials)
        {
            if (possibleMat.name == mode.ToString("g"))
            {
                mat = possibleMat;
                break;
            }
        }
        return mat;
    }
}
