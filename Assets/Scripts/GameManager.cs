﻿using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public List<BaseController> players;
    public List<BaseController> playersRemaining;
    public List<Unit> allUnits;
    public HUDManager hud;
    public SaveManager saveManager;
    SoundManager soundManager;
    public bool hasUsedPower = false;

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
            hasBuffedOne = playerUnits[UnityEngine.Random.Range(0, playerUnits.Count - 1)].TryAddBuff(shieldBuff,false);
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
        foreach (Unit unit in allUnits)
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
            Unit unit = Instantiate(unitType.defaultPrefab).GetComponent<Unit>();
            unit.InitUnit(grid, soundManager, unitInfo);
            allUnits.Add(unit);
        }
    }

    public void ExecutePreSetupMethods()
    {
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
        if (players.Count > 1) return;
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
            StartPlayerTurn();
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

    void StartPlayerTurn(bool isResumingAfterSave = false)
    {
        currentPlayer = playersRemaining[0];
        currentPlayer.StartTurn(!isResumingAfterSave);
    }

    public void UpdateUnitList()
    {
        allUnits = new List<Unit>();
        foreach (BaseController player in players)
        {
            allUnits.AddRange(player.unitsControlled);
            if (player.unitsControlled.Count == 0)
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
