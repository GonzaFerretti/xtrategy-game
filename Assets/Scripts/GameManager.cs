using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public List<BaseController> players;
    public List<BaseController> playersRemaining;
    public List<Unit> allUnits;
    public HUDManager hud;
    SaveManager saveManager;
    SoundManager soundManager;
    public bool hasUsedPower = false;

    public BaseController currentPlayer;
    [SerializeField] GameGridManager grid;

    [SerializeField] int totalTurns;
    [SerializeField] int currentTurn = 1;

    public void Start()
    {
        ExecutePreLoadMethods();
        grid.Init();
        if (saveManager && saveManager.isLoadingFromSave)
        {
            SetupGameFromLoad();
        }
        else
        {
            PrepareAlreadyExistingUnits();
        }
        ExecutePostLoadMethods();
        if (saveManager && saveManager.isLoadingFromSave)
        {
            saveManager.ResetStagedData();
        }
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

    private void ShieldRandomUnit()
    {
        List<Unit> playerUnits = GetHumanPlayerUnits();

        playerUnits[UnityEngine.Random.Range(0, playerUnits.Count - 1)].Shield();
    }

    private void HealAllUnits()
    {
        foreach (var unit in GetHumanPlayerUnits())
        {
            unit.HealCompletely(); 
        }
    }

    List<Unit> GetHumanPlayerUnits()
    {
        return FindObjectOfType<PlayerController>().unitsControlled;
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
            Unit unit = Instantiate(unitInfo.unitType.defaultPrefab).GetComponent<Unit>();
            unit.InitUnit(grid, soundManager, unitInfo);
            allUnits.Add(unit);
        }
    }

    public void ExecutePreLoadMethods()
    {
        CheckGridManagerReferences();
        GetHudReference();
        hud.Init();
        saveManager = FindObjectOfType<SaveManager>();
        soundManager = FindObjectOfType<SoundManager>();
        InitUnitAndPlayerList();
    }

    public void ExecutePostLoadMethods()
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
        saveManager.ProcessDataAndSave(SceneManager.GetActiveScene().name, allUnits, hasUsedPower, currentPlayer is AIController);
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
            SceneManager.LoadScene("Win");
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
