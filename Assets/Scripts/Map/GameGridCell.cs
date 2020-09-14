using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameGridCell : GameGridElement
{
    [SerializeField] Vector3Int coordinates;

    [SerializeField] private Renderer rend;

    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Material startMaterial;
    [SerializeField] private Material endMaterial;
    [SerializeField] private Material pathMaterial;
    [SerializeField] private Material baseMaterial;
    [SerializeField] private Material finalPathMaterial;
    public void SetCoordinates(Vector3Int coords)
    {
        coordinates = coords;
    }
    public Vector3Int GetCoordinates()
    {
        return coordinates;
    }

    public void TintSelected()
    {
        rend.material = selectedMaterial;
    }

    public void TintStart()
    {
        rend.material = startMaterial;
    }

    public void TintEnd()
    {
        rend.material = endMaterial;
    }

    public void TintPath()
    {
        rend.material = pathMaterial;
    }

    public void TintFinalPath()
    {
        rend.material = finalPathMaterial;
    }

    public void Untint()
    {
        rend.material = baseMaterial;
    }
}
