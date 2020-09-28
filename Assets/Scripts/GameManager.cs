using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<BaseController> players;
    public List<BaseController> playersRemaining;
    public List<Unit> allUnits;

    public BaseController currentPlayer;
    [SerializeField] GameGridManager grid;

    [SerializeField] int totalTurns;
    [SerializeField] int currentTurn = 1;

    public void Start()
    {
        CompleteRemaingPlayerList();
        InitGridRefAndUnitList();
        Invoke("StartPlayerTurn", 1);
    }

    public void CompleteRemaingPlayerList()
    {
        playersRemaining = new List<BaseController>();
        for (int i = 0; i < players.Count; i++)
            playersRemaining.Add(players[i]);
    }

    void InitGridRefAndUnitList()
    {
        if (!grid)
        {
            grid = players[0].GetGridReference();
        }
        allUnits = new List<Unit>();
        foreach (BaseController player in players)
        {
            allUnits.AddRange(player.unitsControlled);
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
            else EndMatch();
        }
    }

    void StartPlayerTurn()
    {
        currentPlayer = playersRemaining[0];
        currentPlayer.StartTurn();
    }

    void EndMatch()
    {
       BaseController loserPlayer = players.OrderBy(player => player.GetAmountOfUnitsLeft()).Last();
       loserPlayer.DestroyPlayer();
       // Here should be the UI part of the end of the match
    }
}
