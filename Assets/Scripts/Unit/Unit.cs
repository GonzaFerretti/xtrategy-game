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
    [SerializeField] [HideInInspector] public int movementRange;
    [SerializeField] [HideInInspector] int minAttackRange;
    [SerializeField] [HideInInspector] int maxAttackRange;
    [SerializeField] [HideInInspector] public int damage;
    [SerializeField] public UnitAttributes unitAttributes;
    [SerializeField] [HideInInspector] public AIBehaviour AI;
    [SerializeField] public Animator anim;
    [SerializeField] public GameObject model;

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

    void InitShader()
    {
        rens = GetComponentsInChildren<Renderer>();
        foreach (Renderer ren in rens)
        {
            Material[] usualMat = ren.materials;
            Material[] transMat = new Material[ren.materials.Length];
            for (int i = 0; i < ren.materials.Length; i++)
            {
                Material mat = new Material(seethroughBaseMaterial);
                mat.SetTexture("_Albedo", usualMat[i].GetTexture("_MainTex"));
                mat.SetTexture("_Normal", usualMat[i].GetTexture("_BumpMap"));
                mat.SetFloat("_Glossiness", usualMat[i].GetFloat("_Glossiness"));
                mat.SetFloat("_Metallic", usualMat[i].GetFloat("_Metallic"));
                mat.SetColor("_Color", owner.playerColor);
                transMat[i] = mat;
            }
            ren.materials = transMat;
        }
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
            currentCell = grid.GetCellAtCoordinate(new Vector3Int(Random.Range(1, 10), Random.Range(1, 10), 0));
            transform.position = currentCell.transform.position;
            currentCovers = grid.GetCoversFromCoord(GetCoordinates());
        }
    }

    public virtual void Start()
    {
        SetUnitAttributes();
        //InitShader();
        soundManager = FindObjectOfType<SoundManager>();
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
        grid.EnableCellIndicator(GetCoordinates(), GridIndicatorMode.selectedUnit);
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
        moveState = currentActionState.inProgress;
        anim.Play("move");
        currentCovers = new List<Cover>();
        Vector3Int[] path = grid.CalculateShortestPath(currentCell.GetCoordinates(), destinationCoords);
        Vector3 lastPosition = transform.position;
        for (int i = 0; i < path.Length; i++)
        {
            model.transform.forward = (grid.GetWorldPositionFromCoords(path[i]) - lastPosition).normalized;
            transform.position = grid.GetWorldPositionFromCoords(path[i]);
            lastPosition = transform.position;
            yield return new WaitForSeconds(0.25f);
        }
        possibleMovements = new List<Vector3Int>();
        currentCell = grid.GetCellAtCoordinate(path[path.Length - 1]);
        currentCovers = grid.GetCoversFromCoord(GetCoordinates());
        moveState = currentActionState.ended;
        anim.SetTrigger("endCurrentAnim");
        grid.DisableCellIndicators(possibleMovements);
    }

    public IEnumerator MoveAlongPath(Vector3Int[] givenPath)
    {
        moveState = currentActionState.inProgress;
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
