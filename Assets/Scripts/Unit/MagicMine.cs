﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicMine : GameGridElement
{
    public BaseController owner;

    [HideInInspector] public int stepDamage = 0;

    [HideInInspector] public int centerDetonateDamage = 0;
    [HideInInspector] public int sideDetonateDamage = 0;

    [HideInInspector] public List<Vector3Int> triggerTiles = new List<Vector3Int>();
    [HideInInspector] public List<Vector3Int> detonationTiles = new List<Vector3Int>();

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
