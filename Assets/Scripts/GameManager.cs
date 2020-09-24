using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<BaseController> players;
    public List<BaseController> playersRemaining;

    public BaseController currentPlayer;

    [SerializeField] int totalTurns;
    [SerializeField] int currentTurn = 1;

    public void Start()
    {
        playersRemaining = players;
        StartPlayerTurn();
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
                playersRemaining = players;
                StartPlayerTurn();
            }
            else EndMatch();
        }
    }

    void StartPlayerTurn()
    {
        currentPlayer = playersRemaining[0];
        currentPlayer.ResetUnits();
    }

    void EndMatch()
    {
       BaseController loserPlayer = players.OrderBy(player => player.GetAmountOfUnitsLeft()).Last();
       loserPlayer.DestroyPlayer();
       // Here should be the UI part of the end of the match
    }
}
