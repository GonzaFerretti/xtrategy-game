using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class Unit : GameGridElement
{
    [SerializeField] GameGridCell currentCell;

    [SerializeField] int currentHp;
    [SerializeField] public currentActionState moveState = currentActionState.notStarted;
    [SerializeField] public currentActionState attackState = currentActionState.notStarted;
    [SerializeField] int movementRange;
    [SerializeField] int minAttackRange;
    [SerializeField] int maxAttackRange;
    [SerializeField] public int damage;
    [SerializeField] UnitAttributes unitAttributes;
    [SerializeField] private Renderer rend;

    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Material baseMaterial;

    [SerializeField] private UiHpBar hpBar;

    AsyncRangeQuery currentRangeQuery;
    public List<Vector3Int> possibleMovements;
    public List<Vector3Int> possibleAttacks;

    public void Init(Vector3 startingPos, GameGridCell cell)
    {
        currentCell = cell;
        transform.position = startingPos;
    }

    public Vector3Int GetCoordinates()
    {
        return currentCell.GetCoordinates();
    }

    public void Damage(int amountToDamage)
    {
        currentHp -= amountToDamage;
        if (currentHp <= 0)
        {
            Destroy(hpBar.gameObject);
            Destroy(this.gameObject);
        }
        else
        {
            hpBar.UpdateHPbar(1f * currentHp / (1f * unitAttributes.maxHp));
        }
    }

    public void Update()
    {
        // REMOVE LATER
        if (!currentCell)
        {
            currentCell = grid.GetCellAtCoordinate(new Vector3Int(Random.Range(1, 10), Random.Range(1, 10), 0));
            transform.position = currentCell.transform.position;
        }
    }

    public virtual void Start()
    {
        SetUnitAttributes();
    }

    public virtual void SetUnitAttributes()
    {
        currentHp = unitAttributes.maxHp;
        movementRange = unitAttributes.movementRange;
        minAttackRange = unitAttributes.minAttackRange;
        maxAttackRange = unitAttributes.maxAttackRange;
        damage = unitAttributes.damage;
    }

    public virtual void Select()
    {
        rend.material = selectedMaterial;
        if (moveState == currentActionState.ended) return;
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

    public virtual void PrepareAttack()
    {
        if (possibleAttacks.Count == 0)
        {
            Vector3Int currentCellCoords = currentCell.GetCoordinates();
            currentRangeQuery = grid.QueryUnitAttackRange(minAttackRange, maxAttackRange, currentCellCoords);
            possibleAttacks = new List<Vector3Int>();
            StartCoroutine(WaitForAttackRangeQuery());
        }
        else
        {
            grid.TintBulk(possibleAttacks);
        }
    }

    public IEnumerator Move(Vector3Int destinationCoords)
    {
        if (possibleMovements.Contains(destinationCoords))
        {
            moveState = currentActionState.inProgress;
            Vector3Int[] path = grid.CalculateShortestPath(currentCell.GetCoordinates(), destinationCoords);
            for (int i = 0; i < path.Length; i++)
            {
                transform.position = grid.GetWorldPositionFromCoords(path[i]);
                yield return new WaitForSeconds(0.25f);
            }
            grid.UntintBulk(possibleMovements);
            possibleMovements = new List<Vector3Int>();
            currentCell = grid.GetCellAtCoordinate(path[path.Length - 1]);
            moveState = currentActionState.ended;
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

    public IEnumerator WaitForAttackRangeQuery()
    {
        while (!currentRangeQuery.hasFinished)
        {
            yield return new WaitForEndOfFrame();
        }

        possibleAttacks = currentRangeQuery.cellsInRange;
        grid.TintBulk(possibleAttacks);
        currentRangeQuery.EndQuery();
    }

    public virtual void Deselect()
    {
        rend.material = baseMaterial;
        grid.UntintAll();
    }
}

public enum currentActionState 
{ 
    notStarted,
    inProgress,
    ended
}
