using System.Collections;
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
    bool hasUsedPower = false;

    public BaseController currentPlayer;
    [SerializeField] GameGridManager grid;

    [SerializeField] int totalTurns;
    [SerializeField] int currentTurn = 1;

    public void Start()
    {
        saveManager = FindObjectOfType<SaveManager>();
        if (saveManager.isLoadingFromSave)
        {
            SetupGameFromLoad();
        }
        ExecuteInitMethods();
    }

    void SetupGameFromLoad()
    {
        SaveData saveData = saveManager.Load();
        if (saveData != null)
        {

        }
    }

    public void ExecuteInitMethods()
    {
        CheckGridManagerReferences();
        InitUnitAndPlayerList();
        CheckUnitOwnerReferences();
        GetHudReference();
        CompleteRemaingPlayerList();
        SetUnitMaterials();
        CheckLoser();
        StartPlayerTurn();
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
        allUnits = FindObjectsOfType<Unit>().ToList();
    }

    void CheckUnitOwnerReferences()
    {
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
            uncheckedUnits.Remove(uncheckedUnit);
        }
    }



    public void CompleteRemaingPlayerList()
    {
        playersRemaining = new List<BaseController>();
        for (int i = 0; i < players.Count; i++)
            playersRemaining.Add(players[i]);
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
                CompleteRemaingPlayerList();
                StartPlayerTurn();
            }
            else EndMatchNoTurnsLeft();
        }
    }

    void StartPlayerTurn()
    {
        currentPlayer = playersRemaining[0];
        currentPlayer.StartTurn();
    }

    void EndMatchNoTurnsLeft()
    {
        BaseController loserPlayer = players.OrderBy(player => player.GetAmountOfUnitsLeft()).Last();
        loserPlayer.DestroyPlayer();
    }
}
