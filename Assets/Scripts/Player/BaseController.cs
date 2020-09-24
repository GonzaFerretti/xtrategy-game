﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseController : MonoBehaviour
{
    [SerializeField] public Unit currentlySelectedUnit;
    [SerializeField] protected GameGridManager gridManager;

    [SerializeField] ControllerState[] controllerStates;
    Dictionary<string, ControllerState> runtimeControllerStates = new Dictionary<string, ControllerState>();
    [SerializeField] ControllerState currentState;

    [SerializeField] List<Unit> unitsControlled;

    public virtual void Start()
    {
        InitControllerStates();
    }
    public ControllerState GetCurrentState()
    {
        return currentState;
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

    public virtual void Update()
    {
        currentState.OnUpdate();
    }

    public void ResetUnits()
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
        StartCoroutine(currentlySelectedUnit.Move(target));
    }
}
