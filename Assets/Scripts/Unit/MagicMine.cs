using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicMine : MonoBehaviour
{
    public BaseController owner;
    [HideInInspector] public Vector3Int coordinates;

    public int centerDamage;
    public int sideDamage;

    public int centerDetonateDamage;
    public int sideDetonateDamage;

    public List<Vector3Int> affectedCoordinates = new List<Vector3Int>();

    public void SetTeamColor()
    {
        Renderer meshRen = GetComponent<Renderer>();
        Material baseMaterial = meshRen.material;
        Material modifiedMaterial = Instantiate<Material>(baseMaterial);
        modifiedMaterial.SetColor("_EmissionColor", owner.playerColor);
        modifiedMaterial.SetColor("_Color", owner.playerColor);
        meshRen.material = modifiedMaterial;
    }
}
