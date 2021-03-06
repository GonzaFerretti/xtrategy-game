﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : BaseController
{
    public ItemData currentItem;
    ItemButtonTrigger itemUI;

    bool areInteractionsLocked;

    public List<Vector3Int> tilesInteractionWhiteList = new List<Vector3Int>();

    public void UpdateInteractionLock(bool newStatus)
    {
        areInteractionsLocked = newStatus;
    }

    public void SetItemButtonReference(ItemButtonTrigger reference)
    {
        itemUI = reference;
    }

    public override void Update()
    {
        base.Update();
        CheckMovementAxis();
        CheckZoom();
    }
    public override void SwitchStates(string identifier)
    {
        base.SwitchStates(identifier);
    }

    public void UpdateCurrentItem(ItemData item)
    {
        currentItem = item;
        itemUI.SetUsability(true);
        itemUI.ChangeIcon(item.icon);
    }

    public bool HasItem()
    {
        return currentItem;
    }

    public void UseItem(Unit unit)
    {
        if (currentItem.OnUse(unit))
        { 
            currentItem = null;
            itemUI.SetUsability(false);
            gridManager.gameManager.TriggerTutorialEvent("itemUse");
        }
        else
        {
            // IMPLEMENT ANY FAILURE CONDITIONS HERE!
        }
    }
    public bool GetObjectUnderMouse<T>(out T hitObject, int layerMaskOfObject, bool shouldCheckParent = false) where T : GameGridElement
    {
        hitObject = default(T);
        GameObject hitGameObject = null;
        if (areInteractionsLocked) return false;

        Ray ray = GetCameraController().mainCam.ScreenPointToRay(Input.mousePosition);
        //Debug.DrawLine(ray.origin, ray.origin + ray.direction * 500f, Color.red, 5);
        bool hasHitObject = Physics.Raycast(ray, out RaycastHit hit, 500f, layerMaskOfObject);
        if (hasHitObject)
        {
            hitGameObject = shouldCheckParent ? hit.transform.parent.gameObject : hit.transform.gameObject;
            if (hitGameObject.TryGetComponent<T>(out hitObject))
            {
                if (tilesInteractionWhiteList.Count == 0) return true;

                Vector3Int coordinates = hitObject.GetCoordinates();

                return tilesInteractionWhiteList.Contains(coordinates);
            }
        }
        return false;
    }

    public override void StartTurn(bool shouldRestart = false)
    {
        base.StartTurn(shouldRestart);
        gridManager.gameManager.TriggerTutorialEvent("playerTurnStart");
    }

    public void BindInterfaceLock(GameManager gm)
    {
        gm.OnInterfaceLock += UpdateInteractionLock;
        GetComponent<TouchInputController>().BindInterfaceLock(gm);
    }

    public bool GetButtonState(string identifier, bool shouldConsume)
    {
        if (areInteractionsLocked) return false;

        if (buttonPressStates.ContainsKey(identifier))
        {
            bool initialState = buttonPressStates[identifier];
            if (shouldConsume) buttonPressStates[identifier] = false;
            return initialState;
        }
        return false;
    }

    public bool CheckUnitUISwitch()
    {
        int currentIndex = unitsControlled.IndexOf(currentlySelectedUnit);
        int difference = 0;
        if (GetButtonState("PreviousUnit", true))
        {
            difference = -1;
        }
        else if (GetButtonState("NextUnit", true))
        {
            difference = 1;
        }

        if (difference != 0)
        {
            int nextIndex = currentIndex + difference;
            if (nextIndex >= unitsControlled.Count)
            {
                nextIndex = 0;
            }
            else if (nextIndex < 0 )
            {
                nextIndex = unitsControlled.Count - 1;
            }
            currentlySelectedUnit.Deselect();
            currentlySelectedUnit = null;
            StartCoroutine(WaitForUnitSwitch(unitsControlled[nextIndex]));
            return true;
        }
        return false;
    }

    IEnumerator WaitForUnitSwitch(Unit unitToSwitchTo)
    {
        string startState = GetCurrentStateName();
        while (startState == GetCurrentStateName())
        {
            yield return null;
        }
        currentlySelectedUnit = unitToSwitchTo;
        currentlySelectedUnit.Select();
    }

    public void SetButtonState(string identifier, bool state)
    {
        if (buttonPressStates.ContainsKey(identifier)) buttonPressStates[identifier] = state;
    }

    readonly Dictionary<string, bool> buttonPressStates = new Dictionary<string, bool>();

    public void OnButtonPressDown(string identifier)
    {
        if (buttonPressStates.ContainsKey(identifier))
        {
            buttonPressStates[identifier] = true;
        }
        else
        {
            buttonPressStates.Add(identifier, true);
        }
    }

    public void OnButtonPressUp(string identifier)
    {
        if (buttonPressStates.ContainsKey(identifier))
        {
            buttonPressStates[identifier] = false;
        }
        else
        {
            buttonPressStates.Add(identifier, false);
        }
    }

    // REMOVE LATER
    public void CheckMovementAxis()
    {
        if (areInteractionsLocked) return;

        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            Vector2 movementVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            GetCameraController().MoveCameraByVector(movementVector);
        }
    }

    // REMOVE LATER
    public void CheckZoom()
    {
        if (areInteractionsLocked) return;

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            GetCameraController().ScrollZoom(-Input.GetAxis("Mouse ScrollWheel"));
        }
    }

    public bool CheckUnitDeselect()
    {
        if (GetObjectUnderMouse(out Unit unitSelected, 1 << LayerMask.NameToLayer("Unit")))
        {
            if (!OwnsUnit(unitSelected)) return false;
            currentlySelectedUnit.Deselect();
            currentlySelectedUnit = null;
            StartCoroutine(WaitForUnitSwitch(unitSelected));
            return true;
        }
        else if (!areInteractionsLocked && tilesInteractionWhiteList.Count == 0)
        {
            if (!EventSystem.current.currentSelectedGameObject)
            {
                currentlySelectedUnit.Deselect();
                currentlySelectedUnit = null;

                return true;
            }
            return false;
        }

        return false;
    }

    public void OnHoverGrid(GridIndicatorMode possibleCellsMode, GridIndicatorMode selectedCellsMode, List<Vector3Int> listToCheck)
    {
        // This method only exists because of debugging reasons right now, as the game is currently mobile and there's no hover.
        // Gameplay-wise this was replaced by an indicator that appears beneath a possible target.
        /*
        if (GetObjectUnderMouse(out GameObject objectSelected, 1 << LayerMask.NameToLayer("GroundBase")))
        {
            GameGridCell cell = objectSelected.transform.parent.GetComponent<GameGridCell>();
            if (cell == lastSelectedCoord) return;
            Vector3Int coords = cell.GetCoordinates();
            if (listToCheck.Contains(coords))
            {
                if (lastSelectedCoord) gridManager.EnableCellIndicator(lastSelectedCoord.GetCoordinates(), possibleCellsMode);
                gridManager.EnableCellIndicator(coords, selectedCellsMode);
                lastSelectedCoord = cell;
            }
        }
        else
        {
            if (lastSelectedCoord) gridManager.EnableCellIndicator(lastSelectedCoord.GetCoordinates(), possibleCellsMode);
        }*/
    }
}
