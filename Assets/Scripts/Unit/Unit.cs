using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

public class Unit : GameGridElement
{
    [SerializeField] GameGridCell currentCell;
    [SerializeField] public List<Cover> currentCovers;

    [SerializeField] public int currentHp;
    [SerializeField] public currentActionState moveState = currentActionState.notStarted;
    [SerializeField] public currentActionState attackState = currentActionState.notStarted;
    [SerializeField] public int movementRange;
    [SerializeField] int minAttackRange;
    [SerializeField] int maxAttackRange;
    [SerializeField] public int damage;
    [SerializeField] UnitAttributes unitAttributes;
    [SerializeField] private Renderer rend;
    [SerializeField] public AIBehaviour AI;

    [SerializeField] private UiHpBar hpBar;

    AsyncRangeQuery currentRangeQuery;

    public BaseController owner;
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
            currentCovers = grid.GetCoversFromCoord(GetCoordinates());
        }
    }

    public virtual void Start()
    {
        SetUnitAttributes();
    }

    public void ResetActions()
    {
        attackState = currentActionState.notStarted;
        moveState = currentActionState.notStarted;
        possibleAttacks = new List<Vector3Int>();
        possibleMovements = new List<Vector3Int>();
    }

    public virtual void SetUnitAttributes()
    {
        currentHp = unitAttributes.maxHp;
        movementRange = unitAttributes.movementRange;
        minAttackRange = unitAttributes.minAttackRange;
        maxAttackRange = unitAttributes.maxAttackRange;
        damage = unitAttributes.damage;
        AI = unitAttributes.aiBehaviour;
    }

    public bool HasActionsLeft()
    {
        return moveState != currentActionState.ended || attackState != currentActionState.ended;
    }

    public virtual void Select()
    {
        if (moveState == currentActionState.ended) return;
        if (possibleMovements.Count == 0)
        {
            currentRangeQuery = StartRangeQuery();
            possibleMovements = new List<Vector3Int>();
            StartCoroutine(WaitForRangeQuery());
        }
        else
        {
            grid.EnableCellIndicators(possibleMovements, GridIndicatorMode.possibleMovement);
        }
    }

    public AsyncRangeQuery StartRangeQuery()
    {
        return grid.QueryUnitRange(movementRange, GetCoordinates());
    }

    public AsyncRangeQuery StartAttackRangeQuery()
    {
        return grid.QueryUnitAttackRange(minAttackRange, maxAttackRange, GetCoordinates());
    }

    public virtual void PrepareAttack()
    {
        if (possibleAttacks.Count == 0)
        {
            currentRangeQuery = StartAttackRangeQuery();
            possibleAttacks = new List<Vector3Int>();
            StartCoroutine(WaitForAttackRangeQuery());
        }
        else
        {
            grid.EnableCellIndicators(possibleAttacks, GridIndicatorMode.possibleAttack);
        }
    }

    public IEnumerator MoveByDestinationCoords(Vector3Int destinationCoords)
    {
        moveState = currentActionState.inProgress;
        currentCovers = new List<Cover>();
        Vector3Int[] path = grid.CalculateShortestPath(currentCell.GetCoordinates(), destinationCoords);
        for (int i = 0; i < path.Length; i++)
        {
            transform.position = grid.GetWorldPositionFromCoords(path[i]);
            yield return new WaitForSeconds(0.25f);
        }
        possibleMovements = new List<Vector3Int>();
        currentCell = grid.GetCellAtCoordinate(path[path.Length - 1]);
        currentCovers = grid.GetCoversFromCoord(GetCoordinates());
        moveState = currentActionState.ended;
        grid.DisableCellIndicators(possibleMovements);
    }

    public IEnumerator MoveAlongPath(Vector3Int[] givenPath)
    {
        moveState = currentActionState.inProgress;
        currentCovers = new List<Cover>();
        for (int i = 0; i < givenPath.Length; i++)
        {
            transform.position = grid.GetWorldPositionFromCoords(givenPath[i]);
            yield return new WaitForSeconds(0.25f);
        }
        possibleMovements = new List<Vector3Int>();
        currentCell = grid.GetCellAtCoordinate(givenPath[givenPath.Length - 1]);
        currentCovers = grid.GetCoversFromCoord(GetCoordinates());
        moveState = currentActionState.ended;
    }

    public IEnumerator WaitForRangeQuery()
    {
        while (!currentRangeQuery.hasFinished)
        {
            yield return new WaitForEndOfFrame();
        }

        possibleMovements = currentRangeQuery.cellsInRange;
        grid.EnableCellIndicators(possibleMovements, GridIndicatorMode.possibleMovement);
        currentRangeQuery.EndQuery();
    }

    public IEnumerator WaitForAttackRangeQuery()
    {
        while (!currentRangeQuery.hasFinished)
        {
            yield return new WaitForEndOfFrame();
        }

        possibleAttacks = currentRangeQuery.cellsInRange;
        List<Vector3Int> allyPositionsInRange = possibleAttacks.Intersect(owner.GetOwnedUnitsPosition()).ToList();
        possibleAttacks.RemoveAll(x => allyPositionsInRange.Contains(x));
        grid.EnableCellIndicators(possibleAttacks, GridIndicatorMode.possibleAttack);
        currentRangeQuery.EndQuery();
    }

    public virtual void Deselect()
    {
        grid.DisableAllCellIndicators();
    }
}

public enum currentActionState
{
    notStarted,
    inProgress,
    ended
}
