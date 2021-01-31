using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Unit : GameGridElement
{
    [SerializeField] Vector2Int desiredStartingPos = new Vector2Int(-1, -1);

    [SerializeField] GameGridCell currentCell;
    public List<Cover> currentCovers;

    Dictionary<string, Image> currentlyActiveBuffs = new Dictionary<string, Image>();

    [SerializeField] Image buffIconPrefab;
    [SerializeField] RectTransform buffIconRoot;
    [SerializeField] float buffIconSpacing;

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
    [SerializeField] FloatingHeal healIcon;
    [SerializeField] TextMeshProUGUI typeText;

    public Renderer[] rens;
    public Material seethroughBaseMaterial;

    AsyncRangeQuery currentRangeQuery;

    public BaseController owner;
    public List<Vector3Int> possibleMovements;
    public List<Vector3Int> possibleAttacks;

    public int GetFinalMovementRange()
    {
        return movementRange + (HasBuff("movement") ? unitAttributes.movementBoost : 0);
    }

    public void PlaySound(string name)
    {
        soundManager.Play(sounds.GetSoundClip(name));
    }

    public void PlaySound(SoundClip clip)
    {
        soundManager.Play(clip);
    }

    public string[] GetCurrentlyActiveBuffs()
    {
        return currentlyActiveBuffs.Keys.ToArray();
    }

    public int CalculateFinalDamage()
    {
        return unitAttributes.attackType.CalculateFinalDamage(this);
    }

    public Vector3Int GetCoordinates()
    {
        return currentCell.GetCoordinates();
    }

    public void TakeDamage(int baseDamage, Vector3Int damageSourcePosition)
    {
        bool isCoverInTheWay = false;
        foreach (Cover cover in currentCovers)
        {
            if (cover is HighCover) continue;
            if (grid.IsCoverInTheWayOfAttack(damageSourcePosition, GetCoordinates(), cover))
            {
                isCoverInTheWay = true;
                break;
            }
        }
        if (TryConsumeBuff("shield"))
        {
            PlaySound("shieldhit");
        }
        else
        {
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
                Unit attackingUnit = grid.GetUnitAtCoordinates(damageSourcePosition);
                if (attackingUnit != null)
                    model.transform.forward = (attackingUnit.transform.position - transform.position).normalized;
                anim.Play("hit");
                soundManager.Play(sounds.GetSoundClip("hit"));
                UpdateHpBar();
            }
        }
    }

    public bool HasBuff(string identifier)
    {
        return currentlyActiveBuffs.ContainsKey(identifier);
    }

    public bool TryConsumeBuff(string identifier)
    {
        if (HasBuff(identifier))
        {
            GameObject buffIcon = currentlyActiveBuffs[identifier].gameObject;
            currentlyActiveBuffs.Remove(identifier);
            Destroy(buffIcon);
            UpdateBuffUI();
            return true;
        }
        else return false;

    }

    public bool TryAddBuff(Buff buff)
    {
        if (!HasBuff(buff.identifier))
        {
            currentlyActiveBuffs.Add(buff.identifier, CreateBuffUiIcon(buff.icon));
            UpdateBuffUI();
            return true;
        }
        else return false;
    }

    public void UpdateBuffUI()
    {
        for (int i = 0; i < buffIconRoot.childCount; i++)
        {
            (buffIconRoot.GetChild(i) as RectTransform).anchoredPosition = i * buffIconSpacing * Vector2.left; 
        }
    }

    public Image CreateBuffUiIcon(Sprite iconImage)
    {
        Image newIcon = Instantiate(buffIconPrefab, buffIconRoot);
        newIcon.sprite = iconImage;

        return newIcon;
    }

    public void UpdateHpBar()
    {
        hpBar.UpdateHPbar(1f * currentHp / (1f * unitAttributes.maxHp));
    }

    IEnumerator DestroyBody(GameObject body)
    {
        yield return new WaitForSeconds(3);
        Destroy(body);
    }

    public void InitUnit(GameGridManager gridReference, SoundManager soundMReference, UnitSaveInfo savedInfo = null)
    {
        SetUnitAttributes(savedInfo != null);
        soundManager = soundMReference;
        grid = gridReference;
        SetupAfterLoad(savedInfo);
        InitModel();
        SetupInitialPosition(savedInfo != null);
        typeText.text = unitAttributes.name;
        UpdateHpBar();
    }

    public void HealCompletely()
    {
        currentHp = unitAttributes.maxHp;
        UpdateHpBar();
        StartCoroutine(healIcon.ShowIcon());
    }

    void SetupInitialPosition(bool isLoading)
    {
        if (desiredStartingPos != new Vector2Int(-1, -1) && !isLoading) currentCell = grid.GetCellAtCoordinate(new Vector3Int(desiredStartingPos.x, desiredStartingPos.y, 0));
        if (!currentCell) UpdateCell();
        transform.position = currentCell.transform.position;
        currentCovers = grid.GetCoversFromCoord(GetCoordinates());
    }

    void SetupAfterLoad(UnitSaveInfo savedInfo)
    {
        if (savedInfo == null) return;
        currentHp = savedInfo.hpLeft;
        owner = GameObject.Find(savedInfo.owner).GetComponent<BaseController>();
        foreach (var savedBuff in savedInfo.activeBuffs)
        {
            TryAddBuff(grid.gameManager.saveManager.buffTypeBank.GetBuffType(savedBuff));
        }
        UpdateCell(savedInfo.position);
        attackState = (savedInfo.hasAttacked) ? CurrentActionState.ended : CurrentActionState.notStarted;
        moveState = (savedInfo.hasMoved) ? CurrentActionState.ended : CurrentActionState.notStarted;
    }

    void UpdateCell(Vector3Int coordinates)
    {
        currentCell = grid.GetCellAtCoordinate(coordinates);
        grid.UpdateUnitPositionCache(this, coordinates);
    }

    void UpdateCell()
    {
        currentCell = grid.GetRandomUnusedCell();
        grid.UpdateUnitPositionCache(this, currentCell.GetCoordinates());
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

    public virtual void SetUnitAttributes(bool isSaveLoad)
    {
        if (!isSaveLoad) currentHp = unitAttributes.maxHp;
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
        Camera.main.GetComponent<CameraController>().SetFollowTarget(transform);
        if (moveState == CurrentActionState.ended) return;
        grid.EnableCellIndicator(GetCoordinates(), GridIndicatorMode.selectedUnit);
        grid.SetAllCoverIndicators(true);
        if (possibleMovements.Count == 0)
        {
            ProcessRange(true);
        }
        else
        {
            grid.EnableCellIndicators(possibleMovements, GridIndicatorMode.movementRange);
        }
    }

    public void ProcessRange(bool shouldShowRangeAfterwards)
    {
        currentRangeQuery = StartRangeQuery();
        possibleMovements = new List<Vector3Int>();
        StartCoroutine(WaitForRangeQuery(shouldShowRangeAfterwards));
    }

    public AsyncRangeQuery StartRangeQuery()
    {
        return grid.QueryUnitRange(GetFinalMovementRange(), GetCoordinates());
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
            grid.EnableCellIndicators(possibleAttacks, GridIndicatorMode.attackRange);
            unitAttributes.attackType.CheckAdditionalCellIndicatorsConditions(possibleAttacks, grid, owner as PlayerController);
        }
    }

    public IEnumerator MoveByDestinationCoords(Vector3Int destinationCoords)
    {
        grid.SetAllCoverIndicators(false);
        AsyncPathQuery query = grid.StartShortestPathQuery(currentCell.GetCoordinates(), destinationCoords);

        while (!query.hasFinished)
        {
            yield return null;
        }

        Vector3Int[] path = query.GetPathArray();
        query.End();

        yield return MoveAlongPath(path);
        grid.DisableCellIndicators(possibleMovements);
    }

    public IEnumerator MoveAlongPath(Vector3Int[] givenPath)
    {
        moveState = CurrentActionState.inProgress;
        currentCovers = new List<Cover>();
        anim.Play("move");
        Vector3 lastPosition = transform.position;
        Vector3Int currentCoordinates = GetCoordinates();
        for (int i = 0; i < givenPath.Length; i++)
        {
            model.transform.forward = (grid.GetWorldPositionFromCoords(givenPath[i]) - lastPosition).normalized;
            transform.position = grid.GetWorldPositionFromCoords(givenPath[i]);
            currentCoordinates = givenPath[i];
            if (!unitAttributes.isImmuneToExplosives)
            {
                if (grid.CheckMineProximity(out int damage, currentCoordinates, owner))
                {
                    TakeDamage(damage, currentCoordinates);
                    break;
                }
            }
            lastPosition = transform.position;
            Camera.main.GetComponent<CameraController>().SetFollowTarget(transform);
            yield return new WaitForSeconds(0.25f);
        }
        possibleMovements = new List<Vector3Int>();
        UpdateCell(currentCoordinates);

        if (owner is PlayerController && !(owner as PlayerController).HasItem() && grid.CheckItemAtCoordinate(out ItemPickup outItem, currentCoordinates))
        {
            (owner as PlayerController).UpdateCurrentItem(outItem.itemData);
            grid.DestroyItemPickup(outItem);
        }

        currentCovers = grid.GetCoversFromCoord(GetCoordinates());

        anim.SetTrigger("endCurrentAnim");
        TryConsumeBuff("movement");
        moveState = CurrentActionState.ended;
    }

    public IEnumerator WaitForRangeQuery(bool shouldShowIndicators)
    {
        while (!currentRangeQuery.hasFinished)
        {
            yield return new WaitForEndOfFrame();
        }
        possibleMovements = currentRangeQuery.cellsInRange;


        if (shouldShowIndicators) 
        {
            grid.EnableCellIndicators(possibleMovements, GridIndicatorMode.movementRange);
        }
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
        grid.EnableCellIndicators(possibleAttacks, GridIndicatorMode.attackRange);
        unitAttributes.attackType.CheckAdditionalCellIndicatorsConditions(possibleAttacks, grid, owner as PlayerController);
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
