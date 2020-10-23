﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseController : MonoBehaviour
{
    public Unit currentlySelectedUnit;
    [SerializeField] protected GameGridManager gridManager;

    [SerializeField] ControllerState[] controllerStates;
    readonly Dictionary<string, ControllerState> runtimeControllerStates = new Dictionary<string, ControllerState>();
    [SerializeField] protected ControllerState currentState;

    public List<Unit> unitsControlled;
    public Color playerColor;

    public virtual void Start()
    {
        InitControllerStates();
        SetUnitOwnership();
    }
    public ControllerState GetCurrentState()
    {
        return currentState;
    }

    public IEnumerator WaitForTransitionIn()
    {
        string startingController = GetCurrentStateName();
        while (startingController == GetCurrentStateName())
        {
            yield return null;
        }

        currentState.OnTransitionIn();
    }

    public List<Vector3Int> GetOwnedUnitsPosition()
    {
        List<Vector3Int> unitsPositions = new List<Vector3Int>();
        foreach (Unit unit in unitsControlled)
        {
            unitsPositions.Add(unit.GetCoordinates());
        }
        return unitsPositions;
    }

    public void SetUnitOwnership()
    {
        foreach (Unit unit in unitsControlled)
        {
            unit.owner = this;
        }
    }

    public bool HasAnyMovesLeft()
    {
        bool hasAnyMovesLeft = false;
        foreach (Unit unit in unitsControlled)
        {
            if (unit.HasActionsLeft())
            {
                hasAnyMovesLeft = true;
                break;
            }
        }
        return hasAnyMovesLeft;
    }    

    public GameGridManager GetGridReference()
    {
        return gridManager;
    }

    public void RemoveUnit(Unit unit)
    {
        unitsControlled.Remove(unit);
        gridManager.gameManager.InitGridRefAndUnitList();
    }

    public virtual void InitControllerStates()
    {
        foreach (ControllerState state in controllerStates)
        {
            ControllerState runtimeState = Instantiate(state);
            runtimeState.Init(this);
            runtimeControllerStates.Add(runtimeState.stateName, runtimeState);
            if (state == currentState) currentState = runtimeState;
        }
    }

    public virtual void SwitchStates(string identifier)
    {
        currentState = runtimeControllerStates[identifier];
    }

    public string GetCurrentStateName()
    {
        return currentState.stateName;
    }

    public virtual void Update()
    {
        currentState.OnUpdate();
    }

    public virtual void StartTurn()
    {
        foreach (Unit unit in unitsControlled)
        {
            unit.ResetActions();
        }
    }

    public int GetAmountOfUnitsLeft()
    {
        return unitsControlled.Count;
    }

    public void DestroyPlayer()
    {
        while (unitsControlled.Count > 0)
        {
            GameObject unit = unitsControlled[0].gameObject;
            unitsControlled.RemoveAt(0);
            Destroy(unit);
        }
        Destroy(gameObject);
    }

    public bool OwnsUnit(Unit unit)
    {
        return unitsControlled.Contains(unit);        
    }

    public void MoveUnit(Vector3Int target)
    {
        StartCoroutine(currentlySelectedUnit.MoveByDestinationCoords(target));
    }
}
