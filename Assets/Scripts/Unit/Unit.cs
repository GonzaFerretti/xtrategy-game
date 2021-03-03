using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Unit : GameGridElement
{
    public Vector2Int desiredStartingPos = new Vector2Int(-1, -1);

    [SerializeField] GameGridCell currentCell;
    public List<Cover> currentCovers;

    Dictionary<string, Buff> activeBuffs = new Dictionary<string, Buff>();
    Dictionary<string, BuffIcon> activeBuffsUI = new Dictionary<string, BuffIcon>();

    [SerializeField] BuffIcon buffIconPrefab;
    [SerializeField] RectTransform buffIconRoot;
    [SerializeField] float buffIconSpacing;

    public int currentHp;
    public CurrentActionState moveState = CurrentActionState.notStarted;
    public CurrentActionState attackState = CurrentActionState.notStarted;
    public UnitAttributes attributes;
    [HideInInspector] public Animator anim;
    public GameObject model;

    float lastMovementQueryDistance = -1;

    [SerializeField] SoundManager soundManager;
    [SerializeField] SoundRepository sounds;

    [SerializeField] private UiHpBar hpBar;
    [SerializeField] DamageIndicator damageIndicator;

    [SerializeField] FloatingIcon healIcon;
    [SerializeField] FloatingIcon poisonIcon;
    [SerializeField] TextMeshProUGUI typeText;

    public Renderer[] rens;
    public Material seethroughBaseMaterial;

    AsyncRangeQuery currentRangeQuery;

    public BaseController owner;
    public List<Vector3Int> possibleMovements;
    public List<Vector3Int> possibleAttacks;

    public int GetFinalMovementRange()
    {
        return attributes.movementRange + (HasBuff("movement") ? attributes.movementBoost : 0);
    }

    public void PlaySound(string name)
    {
        soundManager.Play(sounds.GetSoundClip(name));
    }

    public void PlaySound(SoundClip clip)
    {
        soundManager.Play(clip);
    }

    public void ProcessBuffCharges()
    {
        List<string> buffsToConsume = new List<string>();
        foreach (var buffData in activeBuffs)
        {
            Buff buff = buffData.Value;
            if (buff.charges == -1) continue;

            buff.charges--;
            bool shouldConsumeBuff = false;

            if (TryConsumeBuff("shield"))
                shouldConsumeBuff = true;
            else
            {
                // Here I would execute some kind of stragegy method of the Buff to process each charge use.
                // However, there's only one buff that currently uses this and we won't program any other buff.
                // eg. buff.OnTurnTick()
                TakeDamage(1, Vector3Int.right, false, DamageType.reduced);
                shouldConsumeBuff = buff.charges == 0;
                StartCoroutine(poisonIcon.ShowIcon());
                activeBuffsUI[buff.identifier].UpdateAmount(buff.charges);
            }

            if (shouldConsumeBuff)
            {
                buffsToConsume.Add(buffData.Key);
            }
        }

        foreach (var buff in buffsToConsume)
        {
            TryConsumeBuff(buff);
        }
    }

    public List<Buff> GetCurrentlyActiveBuffs()
    {
        return activeBuffs.Values.ToList();
    }

    public int CalculateFinalDamage(bool shouldUseMultiplier = false)
    {
        return attributes.mainAttack.attackType.CalculateFinalDamage(this, shouldUseMultiplier, out DamageType type);
    }

    public override Vector3Int GetCoordinates()
    {
        return currentCell.GetCoordinates();
    }

    public void TakeDamage(int baseDamage, Vector3Int damageSourcePosition, bool isDamageDirectional, DamageType type)
    {
        bool isCoverInTheWay = false;
        if (isDamageDirectional)
        {
            foreach (Cover cover in currentCovers)
            {
                // We could probably handle mines doing damage behind high covers, here.
                if (cover is HighCover) continue;
                if (grid.IsCoverInTheWayOfAttack(damageSourcePosition, GetCoordinates(), cover))
                {
                    isCoverInTheWay = true;
                    break;
                }
            }
        }
        if (TryConsumeBuff("shield"))
        {
            PlaySound("shieldhit");
        }
        else
        {
            // We should probably swap this for a multiplier!!

            int finalDamage = (isCoverInTheWay) ? (baseDamage - 1) : baseDamage;

            currentHp -= finalDamage;

            if (currentHp <= 0)
            {
                soundManager.Play(sounds.GetSoundClip("death"));
                anim.Play("Death");
                CleanDestroy();
            }
            else
            {
                Unit attackingUnit = grid.GetUnitAtCoordinates(damageSourcePosition);
                if (attackingUnit != null)
                    model.transform.forward = (attackingUnit.transform.position - transform.position).normalized;
                anim.Play(isCoverInTheWay && !(attributes is BossAttributes) ? "Cover" : "Hit");
                soundManager.Play(sounds.GetSoundClip("hit"));
                UpdateHpBar();

                DamageType finalDamageType = isCoverInTheWay ? (DamageType)(((int)type) - 1) : type;

                damageIndicator.ShowDamage(finalDamage, finalDamageType);
            }
        }
    }

    public void CleanDestroy(bool destroyModelImmediately = false)
    {
        Destroy(hpBar.gameObject);
        owner.RemoveUnit(this); 
        if (!destroyModelImmediately)
            owner.StartCoroutine(DestroyBody(model));
        else
            Destroy(model);
        Destroy(this);
    }

    public bool HasBuff(string identifier)
    {
        return activeBuffs.ContainsKey(identifier);
    }

    public bool TryConsumeBuff(string identifier)
    {
        if (HasBuff(identifier))
        {
            GameObject buffIcon = activeBuffsUI[identifier].gameObject;
            activeBuffs.Remove(identifier);
            activeBuffsUI.Remove(identifier);
            Destroy(buffIcon);
            UpdateBuffUI();
            return true;
        }
        else return false;

    }

    public bool TryAddBuff(Buff buff, bool shouldInstance)
    {
        if (!HasBuff(buff.identifier))
        {
            activeBuffsUI.Add(buff.identifier, CreateBuffUiIcon(buff));
            buff = (shouldInstance) ? Instantiate(buff) : buff;
            activeBuffs.Add(buff.identifier, buff);
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

    public BuffIcon CreateBuffUiIcon(Buff buff)
    {
        BuffIcon newIcon = Instantiate(buffIconPrefab, buffIconRoot);
        newIcon.Setup(buff.icon, buff.charges);

        return newIcon;
    }

    public void UpdateHpBar()
    {
        hpBar.UpdateHPbar(1f * currentHp / (1f * attributes.maxHp));
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
        if (savedInfo == null) SetupInitialPosition();
        typeText.text = attributes.name;
        UpdateHpBar();
    }

    public bool IsDamaged(float threshold = 1)
    {
        return (currentHp * 1.0f) / (attributes.maxHp * 1.0f) < threshold;
    }

    public void Heal(int amount)
    {
        currentHp = (amount == -1) ? attributes.maxHp : Mathf.Clamp(currentHp + amount, 0, attributes.maxHp);
        UpdateHpBar();
        StartCoroutine(healIcon.ShowIcon());
    }

    void SetupInitialPosition()
    {
        if (desiredStartingPos != new Vector2Int(-1, -1))
        {
            var desiredCoord = new Vector3Int(desiredStartingPos.x, desiredStartingPos.y, 0);
            currentCell = grid.GetCellAtCoordinate(desiredCoord);
            grid.RemoveUnusedCell(desiredCoord, attributes is BossAttributes);
        }
        else
        {
            currentCell = grid.GetRandomUnusedCell();
        }
        
        UpdateCell(GetCoordinates());
    }

    void SetupAfterLoad(UnitSaveInfo savedInfo)
    {
        if (savedInfo == null) return;
        currentHp = savedInfo.hpLeft;
        if (!owner) 
            owner = GameObject.Find(savedInfo.owner).GetComponent<BaseController>();
        foreach (var savedBuff in savedInfo.activeBuffs)
        {
            Buff buff = grid.gameManager.saveManager.buffTypeBank.GetBuffType(savedBuff.identifier);
            if (buff.charges != -1)
            {
                buff = Instantiate(buff);
                buff.charges = savedBuff.remainingCharges;
            }
            TryAddBuff(buff, false);
        }
        UpdateCell(savedInfo.position);
        attackState = (savedInfo.hasAttacked) ? CurrentActionState.ended : CurrentActionState.notStarted;
        moveState = (savedInfo.hasMoved) ? CurrentActionState.ended : CurrentActionState.notStarted;
    }

    public void UpdateCell(Vector3Int coordinates)
    {
        currentCell = grid.GetCellAtCoordinate(coordinates);
        grid.UpdateUnitPositionCache(this, coordinates);
        UpdateCoverData();
        transform.position = grid.GetWorldPositionFromCoords(coordinates);
    }

    public void UpdateCoverData()
    {
        currentCovers = new List<Cover>();
        currentCovers = grid.GetCoversFromCoord(GetCoordinates());
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
        SetupTeamColorMaterial(teamColor);
    }

    public Material mainMaterial;

    protected virtual void SetupTeamColorMaterial(Color teamColor)
    {
        Renderer meshRen = model.transform.GetChild(0).GetComponent<Renderer>();
        Material baseMaterial = meshRen.material;
        mainMaterial = Instantiate<Material>(baseMaterial);

        meshRen.material = mainMaterial;

        if (attributes is BossAttributes) return;

        mainMaterial.SetColor("_Color", teamColor);
    }

    public void ResetActions()
    {
        attackState = CurrentActionState.notStarted;
        moveState = CurrentActionState.notStarted;
        possibleAttacks = new List<Vector3Int>();
        possibleMovements = new List<Vector3Int>();
        lastMovementQueryDistance = -1;
    }

    public virtual void SetUnitAttributes(bool isSaveLoad)
    {
        if (!isSaveLoad) currentHp = attributes.maxHp;
    }

    public bool HasActionsLeft()
    {
        return moveState != CurrentActionState.ended || attackState != CurrentActionState.ended;
    }

    public virtual void Select()
    {
        owner.GetCameraController().SetFollowTarget(transform);
        if (moveState == CurrentActionState.ended) return;
        grid.EnableCellIndicator(GetCoordinates(), GridIndicatorMode.selectedUnit);
        grid.SetAllCoverIndicators(true);
        if (possibleMovements.Count == 0 || lastMovementQueryDistance != GetFinalMovementRange())
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
        lastMovementQueryDistance = GetFinalMovementRange();
        StartCoroutine(WaitForRangeQuery(shouldShowRangeAfterwards));
    }

    public AsyncRangeQuery StartRangeQuery()
    {
        return grid.QueryUnitRange(GetFinalMovementRange(), GetCoordinates(), attributes is BossAttributes);
    }

    public AsyncRangeQuery StartAttackRangeQuery(AttackAttributes attackAttributes = null)
    {
        if (!attackAttributes)
        {
            attackAttributes = attributes.mainAttack;
        }
        return grid.QueryUnitAttackRange(attackAttributes.minAttackRange, attackAttributes.maxAttackRange, GetCoordinates());
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
            EnableAttackCellIndicators();
        }
    }

    void EnableAttackCellIndicators()
    {
        grid.EnableCellIndicators(possibleAttacks, GridIndicatorMode.attackRange);
        attributes.mainAttack.attackType.CheckAdditionalCellIndicatorsConditions(possibleAttacks, grid, owner as PlayerController);
    }

    public IEnumerator MoveByDestinationCoords(Vector3Int destinationCoords)
    {
        grid.SetAllCoverIndicators(false);
        AsyncPathQuery query = grid.StartShortestPathQuery(currentCell.GetCoordinates(), destinationCoords, attributes is BossAttributes);

        while (!query.hasFinished)
        {
            yield return null;
        }

        Vector3Int[] path = query.GetPathArray();
        query.End();

        yield return MoveAlongPath(path);
        grid.DisableCellIndicators(possibleMovements);
        grid.gameManager.TriggerTutorialEvent("unitMove");
    }

    public IEnumerator MoveAlongPath(Vector3Int[] givenPath)
    {
        moveState = CurrentActionState.inProgress;
        currentCovers = new List<Cover>();
        anim.Play("Move");
        Vector3 lastPosition = transform.position;
        Vector3Int currentCoordinates = GetCoordinates();
        for (int i = 0; i < givenPath.Length; i++)
        {
            model.transform.forward = (grid.GetWorldPositionFromCoords(givenPath[i]) - lastPosition).normalized;
            transform.position = grid.GetWorldPositionFromCoords(givenPath[i]);
            currentCoordinates = givenPath[i];
            if (!attributes.isImmuneToExplosives)
            {
                if (grid.CheckMineProximity(out int damage, currentCoordinates, owner))
                {
                    TakeDamage(damage, currentCoordinates, true, DamageType.normal);
                    break;
                }
            }
            lastPosition = transform.position;
            owner.GetCameraController().SetFollowTarget(transform);
            yield return new WaitForSeconds(0.25f);
        }
        possibleMovements = new List<Vector3Int>();
        
        UpdateCell(currentCoordinates);

        if (owner is PlayerController && !(owner as PlayerController).HasItem() && grid.CheckItemAtCoordinate(out ItemPickup outItem, currentCoordinates))
        {
            (owner as PlayerController).UpdateCurrentItem(outItem.itemData);
            grid.gameManager.TriggerTutorialEvent("itemObtained");
            grid.DestroyItemPickup(outItem);
        }

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
        if (!attributes.mainAttack.attackType.shouldAllowAllyTargeting)
        {
            List<Vector3Int> allyPositionsInRange = possibleAttacks.Intersect(owner.GetOwnedUnitsPosition()).ToList();
            possibleAttacks.RemoveAll(x => allyPositionsInRange.Contains(x));
        }
        EnableAttackCellIndicators();
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
