using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : GameGridElement
{
    [SerializeField] GameGridCell currentCell;

    [SerializeField] float currentHp;
    [SerializeField] bool canMove = true;
    [SerializeField] bool canAttack = true;
    [SerializeField] int movementRange;
    [SerializeField] UnitAttributes unitAttributes;
    [SerializeField] private Renderer rend;

    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Material baseMaterial;

    public virtual void Start()
    {
        SetUnitAttributes();
    }

    public virtual void SetUnitAttributes()
    {
        currentHp = unitAttributes.maxHp;
        movementRange = unitAttributes.movementRange;
    }

    public virtual void Select()
    {
        rend.material = selectedMaterial;
        currentCell.TintSelected();
    }

    public virtual void Deselect()
    {
        rend.material = baseMaterial;
        currentCell.Untint();
    }
}
