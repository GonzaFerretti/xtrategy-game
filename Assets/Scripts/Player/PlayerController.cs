using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : BaseController
{
    public override void Update()
    {
        base.Update();
        CheckMovementAxis();
        CheckZoom();
    }
    public override void SwitchStates(string identifier)
    {
        lastSelectedCoord = null;
        base.SwitchStates(identifier);
    }

    GameGridCell lastSelectedCoord = null;
    public bool GetObjectUnderMouse(out GameObject hitObject, int layerMaskOfObject)
    {
        hitObject = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawLine(ray.origin, ray.origin + ray.direction * 500f, Color.red, 5);
        bool hasHitObject = Physics.Raycast(ray, out RaycastHit hit, 500f, layerMaskOfObject);
        if (hasHitObject)
        {
            hitObject = hit.transform.gameObject;
        }
        return hasHitObject;
    }

    public override void StartTurn()
    {
        base.StartTurn();
        if (GetCurrentStateName() == "waitForTurn")
        {
            currentState.ForceFirstTransition();
        }
    }

    public bool GetButtonState(string identifier)
    {
        return buttonPressStates.ContainsKey(identifier) ? buttonPressStates[identifier] : false;
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
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            Vector2 movementVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            Camera.main.GetComponent<CameraController>().MoveCamera(movementVector);
        }
    }

    // REMOVE LATER
    public void CheckZoom()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            Camera.main.GetComponent<CameraController>().ScrollZoom(-Input.GetAxis("Mouse ScrollWheel"));
        }
    }

    public bool CheckUnitDeselect()
    {
        if (GetObjectUnderMouse(out GameObject objectSelected, 1 << LayerMask.NameToLayer("Unit")))
        {
            Unit unitSelected = objectSelected.GetComponent<Unit>();
            if (!OwnsUnit(unitSelected)) return false;
            currentlySelectedUnit.Deselect();
            currentlySelectedUnit = unitSelected;
            currentlySelectedUnit.Select();
            return true;
        }
        else
        {
            if (!EventSystem.current.currentSelectedGameObject)
            {
                currentlySelectedUnit.Deselect();
                currentlySelectedUnit = null;

                return true;
            }
            return false;
        }
    }

    public void OnHoverGrid(GridIndicatorMode possibleCellsMode, GridIndicatorMode selectedCellsMode, List<Vector3Int> listToCheck)
    {
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
        }
    }
}
