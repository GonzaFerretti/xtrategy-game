using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Unit : GameGridElement
{
    [SerializeField] GameGridCell currentCell;
    public List<Cover> currentCovers;

    public int currentHp;
    public CurrentActionState moveState = CurrentActionState.notStarted;
    public CurrentActionState attackState = CurrentActionState.notStarted;
    [HideInInspector] public int movementRange;
    [HideInInspector] int minAttackRange;
    [HideInInspector] int maxAttackRange;
    [HideInInspector] public int damage;
    public UnitAttributes unitAttributes;
    [HideInInspector] public AIBehaviour AI;
    [HideInInspector] public Animator anim;
    public GameObject model;

    [SerializeField] SoundManager soundManager;
    [SerializeField] SoundRepository sounds;

    [SerializeField] private UiHpBar hpBar;

    public Renderer[] rens;
    public Material seethroughBaseMaterial;

    AsyncRangeQuery currentRangeQuery;

    public BaseController owner;
    public List<Vector3Int> possibleMovements;
    public List<Vector3Int> possibleAttacks;

    public void PlaySound(string name)
    {
        soundManager.Play(sounds.GetSoundClip(name));
    }

    public void PlaySound(SoundClip clip)
    {
        soundManager.Play(clip);
    }

    public Vector3Int GetCoordinates()
    {
        return currentCell.GetCoordinates();
    }

    public void TakeDamage(int baseDamage, Unit attackingUnit)
    {
        bool isCoverInTheWay = false;
        foreach (Cover cover in currentCovers)
        {
            if (cover is HighCover) continue;
            if (grid.IsCoverInTheWayOfAttack(attackingUnit.GetCoordinates(), GetCoordinates(), cover))
            {
                isCoverInTheWay = true;
                break;
            }
        }
        currentHp = (isCoverInTheWay) ? currentHp - (baseDamage - 1) : currentHp - baseDamage;
        if (currentHp <= 0)
        {
            Destroy(hpBar.gameObject);
            soundManager.Play(sounds.GetSoundClip("death"));
            anim.Play("death");
            owner.RemoveUnit(this);
            owner.StartCoroutine(DestroyBody(model));
            Destroy(this);
        }
        else
        {
            model.transform.forward = (attackingUnit.transform.position - transform.position).normalized;
            anim.Play("hit");
            soundManager.Play(sounds.GetSoundClip("hit"));
            hpBar.UpdateHPbar(1f * currentHp / (1f * unitAttributes.maxHp));
        }
    }

    IEnumerator DestroyBody(GameObject body)
    {
        yield return new WaitForSeconds(3);
        Destroy(body);
    }

    public void Update()
    {
        // REMOVE LATER
        if (!currentCell)
        {
            currentCell = grid.GetRandomUnusedCell();
            transform.position = currentCell.transform.position;
            currentCovers = grid.GetCoversFromCoord(GetCoordinates());
        }
    }

    public virtual void Start()
    {
        SetUnitAttributes();
        //InitShader();
        soundManager = FindObjectOfType<SoundManager>();
        if (!grid) grid = FindObjectOfType<GameGridManager>();
        InitModel();
    }

    void InitModel()
    {
        GameObject go = Instantiate(model, transform);
        go.transform.localPosition = Vector3.zero;
        anim = go.GetComponent<Animator>();
        model = go;
    }

    public void SetTeamColor(Color teamColor)
    {
        StartCoroutine(WaitForModelAndSetTeamColor(teamColor));
    }

    IEnumerator WaitForModelAndSetTeamColor(Color teamColor)
    {
        while (model.gameObject.scene.name == null)
        {
            yield return null;
        }
        Renderer meshRen = model.transform.GetChild(1).GetComponent<Renderer>();
        Material baseMaterial = meshRen.material;
        Material modifiedMaterial = Instantiate<Material>(baseMaterial);
        modifiedMaterial.SetColor("_Color", teamColor);
        meshRen.material = modifiedMaterial;
    }

    public void ResetActions()
    {
        attackState = CurrentActionState.notStarted;
        moveState = CurrentActionState.notStarted;
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
        return moveState != CurrentActionState.ended || attackState != CurrentActionState.ended;
    }

    public virtual void Select()
    {
        if (moveState == CurrentActionState.ended) return;
        grid.EnableCellIndicator(GetCoordinates(), GridIndicatorMode.selectedUnit);
        grid.SetAllCoverIndicators(true);
        Camera.main.GetComponent<CameraController>().SetFollowTarget(transform);
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
        grid.EnableCellIndicator(GetCoordinates(), GridIndicatorMode.selectedUnit);
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
        grid.SetAllCoverIndicators(false);
        moveState = CurrentActionState.inProgress;
        anim.Play("move");
        currentCovers = new List<Cover>();

        AsyncPathQuery query = grid.StartShortestPathQuery(currentCell.GetCoordinates(), destinationCoords);

        while (!query.hasFinished)
        {
            yield return null;
        }

        Vector3Int[] path = query.GetPathArray();

        query.End();

        Vector3 lastPosition = transform.position;
        for (int i = 0; i < path.Length; i++)
        {
            model.transform.forward = (grid.GetWorldPositionFromCoords(path[i]) - lastPosition).normalized;
            transform.position = grid.GetWorldPositionFromCoords(path[i]);
            lastPosition = transform.position;
            Camera.main.GetComponent<CameraController>().SetFollowTarget(transform);
            yield return new WaitForSeconds(0.25f);
        }
        possibleMovements = new List<Vector3Int>();
        currentCell = grid.GetCellAtCoordinate(path[path.Length - 1]);
        currentCovers = grid.GetCoversFromCoord(GetCoordinates());
        moveState = CurrentActionState.ended;
        anim.SetTrigger("endCurrentAnim");
        grid.DisableCellIndicators(possibleMovements);
    }

    public IEnumerator MoveAlongPath(Vector3Int[] givenPath)
    {
        moveState = CurrentActionState.inProgress;
        currentCovers = new List<Cover>();
        anim.Play("move");
        Vector3 lastPosition = transform.position;
        for (int i = 0; i < givenPath.Length; i++)
        {
            model.transform.forward = (grid.GetWorldPositionFromCoords(givenPath[i]) - lastPosition).normalized;
            transform.position = grid.GetWorldPositionFromCoords(givenPath[i]);
            yield return new WaitForSeconds(0.25f);
        }
        possibleMovements = new List<Vector3Int>();
        currentCell = grid.GetCellAtCoordinate(givenPath[givenPath.Length - 1]);
        currentCovers = grid.GetCoversFromCoord(GetCoordinates());

        anim.SetTrigger("endCurrentAnim");
        moveState = CurrentActionState.ended;
    }

    public IEnumerator WaitForRangeQuery()
    {
        while (!currentRangeQuery.hasFinished)
        {
            yield return new WaitForEndOfFrame();
        }

        possibleMovements = currentRangeQuery.cellsInRange;
        grid.EnableCellIndicators(possibleMovements, GridIndicatorMode.possibleMovement);
        currentRangeQuery.End();
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
        currentRangeQuery.End();
    }

    public virtual void Deselect()
    {
        grid.DisableAllCellIndicators();
        grid.SetAllCoverIndicators(false);
    }
}

public enum CurrentActionState
{
    notStarted,
    inProgress,
    ended
}
