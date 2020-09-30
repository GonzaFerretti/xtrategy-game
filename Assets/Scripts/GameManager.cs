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

    public BaseController currentPlayer;
    [SerializeField] GameGridManager grid;

    [SerializeField] int totalTurns;
    [SerializeField] int currentTurn = 1;

    public void Start()
    {
        CompleteRemaingPlayerList();
        InitGridRefAndUnitList();
        StartPlayerTurn();
    }

    public void Update()
    {
        CheckForMenuKey();
    }

    public void CompleteRemaingPlayerList()
    {
        playersRemaining = new List<BaseController>();
        for (int i = 0; i < players.Count; i++)
            playersRemaining.Add(players[i]);
    }

    public void InitGridRefAndUnitList()
    {
        if (!grid)
        {
            grid = players[0].GetGridReference();
        }
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

    public void CheckForMenuKey()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            hud.SwitchMenu();
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
