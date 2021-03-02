using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public delegate void TutorialEventDelegate(string identifier);
    public delegate void InterfaceLockDelegate(bool newState);

    public List<BaseController> players;
    public List<BaseController> playersRemaining;
    public List<Unit> allUnits;
    public HUDManager hud;
    public SaveManager saveManager;
    SoundManager soundManager;
    public bool hasUsedPower = false;
    public CameraController camController;

    [Header("Tutorial")]
    public bool isTutorial = false;
    public TutorialEnemyFlags enemyFlags;
    public TutorialEventDelegate OnTutorialEvent;
    public InterfaceLockDelegate OnInterfaceLock;

    public BaseController currentPlayer;
    [SerializeField] GameGridManager grid;

    [SerializeField] int totalTurns;
    [SerializeField] int currentTurn = 1;

    public IEnumerator InitiateGame(LoadingScreen loadingScreen)
    {
        // I know this method of hardcoding current level load isn't ideal but I wanted a cheap way 
        // to show some progress to the user without having to build too much infrastructure for it.

        // It also forces the game init to take at least 5 frames to allow for the UI to update properly inbetween.
        // This is to be expected.

        ExecutePreSetupMethods();
        loadingScreen.inLevelSetupProgress = 1f / 5;
        yield return null;
        grid.Init();
        loadingScreen.inLevelSetupProgress = 2f / 5;
        yield return null;
        if (saveManager && saveManager.isLoadingFromSave)
        {
            SetupGameFromLoad();
        }
        else
        {
            PrepareAlreadyExistingUnits();
            grid.SetupNewItems();
        }
        loadingScreen.inLevelSetupProgress = 3f / 5;
        yield return null;
        ExecutePostSetupMethods();
        loadingScreen.inLevelSetupProgress = 4f / 5;
        yield return null;
        if (saveManager && saveManager.isLoadingFromSave)
        {
            saveManager.ResetStagedData();
        }
        loadingScreen.inLevelSetupProgress = 1;
        yield return null;
        CheckTutorial();
    }

    public void TriggerTutorialEvent(string identifier)
    {
        if (OnTutorialEvent != null)
            OnTutorialEvent.Invoke(identifier);
    }

    public GameObject SpawnTutorialElement(TutorialElementsSpawnData tutorialSpawnData, TutorialElementType type)
    {
        Vector3Int coordinates = tutorialSpawnData.coordinates;
        GameObject go = null;

        switch (type)
        {
            case TutorialElementType.cover:
                {
                    var coverData = tutorialSpawnData.coverData;
                    go = grid.CreateSingleCover(coverData.coverData, coverData.coverPrefabName).gameObject;

                    grid.GenerateAllPossibleNeighbours();

                    break;
                }
            case TutorialElementType.item:
                {
                    go = grid.SetupSingleItem(tutorialSpawnData.itemData, coordinates).gameObject;
                    break;
                }
            case TutorialElementType.unit:
                {
                    UnitOwnership ownership = tutorialSpawnData.isPlayerOwned ? UnitOwnership.player : UnitOwnership.ai;
                    Unit newUnit = CreateSingleUnit(tutorialSpawnData.unitData, ownership:ownership);
                    newUnit.UpdateCell(coordinates);
                    allUnits.Add(newUnit);

                    CheckUnitOwnerReferences();
                    SetUnitMaterials();

                    go = newUnit.gameObject;

                    if (tutorialSpawnData.forceFaceCamera)
                    {
                        Vector3 camPos = camController.transform.position;
                        Vector3 unitPos = newUnit.transform.position;

                        Vector3 groundPos = new Vector3(camPos.x, unitPos.y, camPos.z);
                        Vector3 dir = (groundPos - unitPos).normalized;

                        newUnit.transform.forward = dir;
                    }

                    break;
                }
        }

        return go;
    }

    public void DestroyTutorialElement(GameObject objectToDestroy, TutorialElementType type)
    {
        switch (type)
        {
            case TutorialElementType.cover:
                {
                    var cover = objectToDestroy.GetComponent<Cover>();
                    grid.DestroyCover(cover);
                    break;
                }
            case TutorialElementType.item:
                {
                    var itemPickup = objectToDestroy.GetComponent<ItemPickup>();
                    grid.DestroyItemPickup(itemPickup);
                    break;
                }
            case TutorialElementType.unit:
                {
                    var unit = objectToDestroy.GetComponent<Unit>();
                    unit.CleanDestroy(true);
                    break;
                }
        }
    }

    public int GetTurnNumber()
    {
        return currentTurn;
    }

    public void UsePower(string type)
    {
        if (type == "video")
        {
            HealAllUnits();
        }
        else
        {
            ShieldRandomUnit();
        }

        SetPowerUsageStatus(true);
    }

    public PlayerController GetPlayer()
    {
        foreach (var player in players)
        {
            if (player is PlayerController)
            {
                return (player as PlayerController);
            }
        }
        return null;
    }

    ItemData GetPlayerCurrentItem()
    {
        PlayerController player = GetPlayer();
        if (!player) return null;
        return player.currentItem;
    }

    private void ShieldRandomUnit()
    {
        List<Unit> playerUnits = GetHumanPlayerUnits();
        bool hasBuffedOne = false;
        int currentIndex = -1;
        Buff shieldBuff = saveManager.buffTypeBank.GetBuffType("shield");
        do
        {
            hasBuffedOne = playerUnits[UnityEngine.Random.Range(0, playerUnits.Count - 1)].TryAddBuff(shieldBuff, false);
            currentIndex++;
        } while (!hasBuffedOne || currentIndex < playerUnits.Count);
    }

    private void HealAllUnits()
    {
        foreach (var unit in GetHumanPlayerUnits())
        {
            unit.Heal(-1);
        }
    }

    List<Unit> GetHumanPlayerUnits()
    {
        return GetPlayer().unitsControlled;
    }

    void SanitizeControllerUnitList()
    {
        foreach (var player in players)
        {
            player.unitsControlled.Clear();
        }
    }

    void PrepareAlreadyExistingUnits()
    {
        allUnits = FindObjectsOfType<Unit>().ToList();
        var unitsToSetFirst = new List<Unit>();
        var unitsToSetLast = new List<Unit>();
        var defaultPos = new Vector2Int(-1, -1);

        foreach (var unit in allUnits)
        {
            if (unit.desiredStartingPos != defaultPos)
                unitsToSetFirst.Add(unit);
            else
                unitsToSetLast.Add(unit);
        }

        foreach (Unit unit in unitsToSetFirst)
        {
            unit.InitUnit(grid, soundManager);
        }

        foreach (Unit unit in unitsToSetLast)
        {
            unit.InitUnit(grid, soundManager);
        }
    }

    public void CleanExistingUnits()
    {
        foreach (var unit in FindObjectsOfType<Unit>())
        {
            Destroy(unit.gameObject);
        }
    }

    void SetupGameFromLoad()
    {
        SaveData saveData = saveManager.stagedSaveDataToLoad;
        if (saveData != null)
        {
            CleanExistingUnits();
            LoadUnits(saveData);
            SetPowerUsageStatus(saveData.hasUsedPower);
            SetTurn(saveData.isEnemyTurn);
            SanitizeControllerUnitList();
            LoadMines(saveData.mines);
            LoadPickupItems(saveData.pickupItems);
            SetPlayerCurrentItem(saveData.currentPlayerItemId);
        }
    }

    void SetPlayerCurrentItem(int itemId)
    {
        PlayerController player = GetPlayer();
        if (player && itemId != -1)
        {
            player.UpdateCurrentItem(saveManager.itemTypeBank.GetItemType(itemId));
        }
    }

    void LoadPickupItems(PickupItemSaveInfo[] pickupItemSaveInfo)
    {
        foreach (var pickupItem in pickupItemSaveInfo)
        {

            ItemData itemType = saveManager.itemTypeBank.GetItemType(pickupItem.itemId);
            grid.SetupSingleItem(itemType, pickupItem.position);
        }
    }

    void LoadMines(MineSaveInfo[] mineSaveInfos)
    {
        foreach (var mineSaveInfo in mineSaveInfos)
        {
            grid.InitLoadedMine(mineSaveInfo);
        }
    }

    void SetPowerUsageStatus(bool hasUsedPower = false)
    {
        this.hasUsedPower = hasUsedPower;
        if (hasUsedPower && hud != null)
        {
            hud.DisableAdButton();
        }
    }

    void SetTurn(bool isEnemyTurn)
    {
        currentPlayer = (isEnemyTurn) ? (BaseController)FindObjectOfType<AIController>() : (BaseController)FindObjectOfType<PlayerController>();
    }

    void LoadUnits(SaveData saveData)
    {
        foreach (var unitInfo in saveData.units)
        {
            UnitAttributes unitType = saveManager.unitTypeBank.GetUnitType(unitInfo.unitId);
            CreateSingleUnit(unitType, unitInfo);
        }
    }

    public enum UnitOwnership
    {
        player,
        ai,
        none,
    }

    private Unit CreateSingleUnit(UnitAttributes unitType, UnitSaveInfo unitInfo = null, UnitOwnership ownership = UnitOwnership.none)
    {
        Unit unit = Instantiate(unitType.defaultPrefab).GetComponent<Unit>();

        // I know this is lazy, but I know I'll only ever use this for the tutorial.
        if (ownership != UnitOwnership.none)
        {
            string ownerName = (ownership == UnitOwnership.player) ? "PlayerController" : "AIController";
            unit.owner = GameObject.Find(ownerName).GetComponent<BaseController>();
        }

        unit.InitUnit(grid, soundManager, unitInfo);
        allUnits.Add(unit);
        return unit;
    }

    public void ExecutePreSetupMethods()
    {
        camController = Camera.main.GetComponent<CameraController>();
        CheckGridManagerReferences();
        GetHudReference();
        hud.Init();
        saveManager = FindObjectOfType<SaveManager>();
        soundManager = FindObjectOfType<SoundManager>();
        InitUnitAndPlayerList();
    }

    public void ExecutePostSetupMethods()
    {
        CheckUnitOwnerReferences();
        CompleteRemaingPlayerList();
        SetUnitMaterials();
        ExecuteControllerStarts();
        CheckLoser();
        StartPlayerTurn(saveManager ? saveManager.isLoadingFromSave : false);
    }

    void CheckTutorial()
    {
        TutorialManager tutorialManager = FindObjectOfType<TutorialManager>();
        if (tutorialManager)
        {
            tutorialManager.StartTutorial();
            isTutorial = true;
        }
    }

    private void ExecuteControllerStarts()
    {
        foreach (var controller in players)
        {
            controller.Init();
        }
    }

    public void SaveMatch()
    {
        PreSaveData dataToProcess = new PreSaveData
        {
            levelName = SceneManager.GetActiveScene().name,
            units = allUnits,
            hasUsedPower = hasUsedPower,
            isEnemyTurn = currentPlayer is AIController,
            mines = FindObjectsOfType<MagicMine>().ToList(),
            pickupItems = FindObjectsOfType<ItemPickup>().ToList(),
            playerCurrentItem = GetPlayerCurrentItem()
        };

        saveManager.ProcessDataAndSave(dataToProcess);
    }

    void GetHudReference()
    {
        hud = FindObjectOfType<HUDManager>();
    }

    void CheckGridManagerReferences()
    {
        if (!grid) grid = FindObjectOfType<GameGridManager>();
        if (!grid.gameManager) grid.gameManager = this;
    }

    void SetUnitMaterials()
    {
        foreach (Unit unit in allUnits)
        {
            unit.SetTeamColor(unit.owner.playerColor);
        }
    }

    void InitUnitAndPlayerList()
    {
        if (players.Count == 0)
            players = FindObjectsOfType<BaseController>().ToList();
        foreach (BaseController player in players)
        {
            player.SetGridReference(grid);
            if (player is PlayerController)
            {
                (player as PlayerController).BindInterfaceLock(this);
            }
        }
    }

    void CheckUnitOwnerReferences()
    {
        foreach (BaseController uncheckedController in players)
        {
            uncheckedController.unitsControlled.RemoveAll(item => item == null);
        }

        List<Unit> uncheckedUnits = new List<Unit>();

        foreach (Unit unit in allUnits)
        {
            uncheckedUnits.Add(unit);
        }

        while (uncheckedUnits.Count > 0)
        {
            Unit uncheckedUnit = uncheckedUnits[0];
            if (!uncheckedUnit.owner)
            {
                foreach (BaseController uncheckedController in players)
                {
                    foreach (Unit unit in uncheckedController.unitsControlled)
                    {
                        if (uncheckedUnit == unit)
                        {
                            uncheckedUnit.owner = uncheckedController;
                            break;
                        }
                    }
                    if (uncheckedUnit.owner) break;
                }
            }
            else
            {
                foreach (BaseController uncheckedController in players)
                {
                    if (uncheckedController == uncheckedUnit.owner)
                    {
                        if (!uncheckedController.unitsControlled.Contains(uncheckedUnit))
                            uncheckedController.unitsControlled.Add(uncheckedUnit);

                        break;
                    }
                }
            }
            uncheckedUnits.Remove(uncheckedUnit);
        }
    }


    public void CompleteRemaingPlayerList()
    {
        playersRemaining = new List<BaseController>();
        int remainingIndex = 0;
        int playersIndex = 0;

        if (currentPlayer)
        {
            playersRemaining.Insert(0, currentPlayer);
            remainingIndex = 1;
            players.Remove(currentPlayer);
        }

        do
        {
            playersRemaining.Add(players[playersIndex]);
            remainingIndex++;
            playersIndex++;
        } while (remainingIndex < players.Count);

        if (currentPlayer)
        {
            players.Insert(0, currentPlayer);
        }
    }

    public void CheckLoser()
    {
        if (players.Count > 1 || isTutorial) return;
        if (players[0] is PlayerController)
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            string nextScene = SceneUtility.GetScenePathByBuildIndex(currentSceneIndex + 1);
            if (nextScene.Contains("Level"))
            {
                saveManager.StageLoadForCleanLevel("Level" + (currentSceneIndex + 1));
            }
            else
            {
                SceneManager.LoadScene("Win");
            }
        }
        else
        {
            SceneManager.LoadScene("GameOver");
        }
    }

    public void EndPlayerTurn()
    {
        if (playersRemaining.Count > 1)
        {
            playersRemaining.Remove(playersRemaining[0]);
            StartPlayerTurn(shouldSkipStart: playersRemaining[0] is AIController && !enemyFlags.canEnemyHaveTurn);
        }
        else
        {
            if (currentTurn < totalTurns)
            {
                currentTurn++;
                currentPlayer = null;
                CompleteRemaingPlayerList();
                StartPlayerTurn();
            }
            else EndMatchNoTurnsLeft();
        }
    }

    void StartPlayerTurn(bool isResumingAfterSave = false, bool shouldSkipStart = false)
    {
        currentPlayer = playersRemaining[0];

        if (shouldSkipStart) return;

        currentPlayer.StartTurn(!isResumingAfterSave);
    }

    public void UpdateUnitList()
    {
        allUnits = new List<Unit>();
        foreach (BaseController player in players)
        {
            allUnits.AddRange(player.unitsControlled);
            if (player.unitsControlled.Count == 0 && !isTutorial)
            {
                players.Remove(player);
                break;
            }
        }
        CheckLoser();
    }

    void EndMatchNoTurnsLeft()
    {
        BaseController loserPlayer = players.OrderBy(player => player.GetAmountOfUnitsLeft()).Last();
        loserPlayer.DestroyPlayer();
    }
}

[System.Serializable]
public struct TutorialEnemyFlags
{
    public bool canEnemyAttack;
    public bool canEnemyMove;
    public bool canEnemyHaveTurn;
}
