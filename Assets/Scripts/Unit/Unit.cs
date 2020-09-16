using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
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

    AsyncRangeQuery currentRangeQuery;
    public List<Vector3Int> possibleMovements;

    public void Init(Vector3 startingPos, GameGridCell cell)
    {
        currentCell = cell;
        transform.position = startingPos;
    }

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
        if (possibleMovements.Count == 0)
        {
            Vector3Int currentCellCoords = currentCell.GetCoordinates();
            currentRangeQuery = grid.QueryUnitRange(movementRange, currentCellCoords);
            possibleMovements = new List<Vector3Int>();
            StartCoroutine(WaitForRangeQuery());
        }
        else
        {
            grid.TintBulk(possibleMovements);
        }
    }

    public IEnumerator Move(Vector3Int destinationCoords)
    {
        if (possibleMovements.Contains(destinationCoords))
        {
            Vector3Int[] path = grid.CalculateShortestPath(currentCell.GetCoordinates(), destinationCoords);
            for (int i = 0; i < path.Length; i++)
            {
                transform.position = grid.GetWorldPositionFromCoords(path[i]);
                yield return new WaitForSeconds(0.25f);
            }
            grid.UntintBulk(possibleMovements);
            possibleMovements = new List<Vector3Int>();
            currentCell = grid.GetCellAtCoordinate(path[path.Length - 1]);
        }
    }

    public IEnumerator WaitForRangeQuery()
    {
        while (!currentRangeQuery.hasFinished)
        {
            yield return new WaitForEndOfFrame();
        }

        possibleMovements = currentRangeQuery.cellsInRange;
        grid.TintBulk(possibleMovements);
        currentRangeQuery.EndQuery();
    }

    public virtual void Deselect()
    {
        rend.material = baseMaterial;
        grid.UntintBulk(possibleMovements);
    }
}
