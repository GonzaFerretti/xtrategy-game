using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseController : MonoBehaviour
{
    [SerializeField] public Unit currentlySelectedUnit;
    [SerializeField] protected GameGridManager gridManager;

    [SerializeField] ControllerState[] controllerStates;
    Dictionary<string, ControllerState> runtimeControllerStates = new Dictionary<string, ControllerState>();
    [SerializeField] ControllerState currentState;

    public virtual void Start()
    {
        InitControllerStates();
    }
    public ControllerState GetCurrentState()
    {
        return currentState;
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

    public void MoveUnit(Vector3Int target)
    {
        StartCoroutine(currentlySelectedUnit.Move(target));
    }
}
