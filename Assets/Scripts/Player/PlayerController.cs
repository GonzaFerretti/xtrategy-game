﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : BaseController
{
    public void Update()
    {
        if (Input.GetMouseButtonDown(0)) CheckUnitSelect();

    }

    void CheckPlayerActions()
    {

    }

    void CheckUnitSelect()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 mousePosition = ray.origin;
        RaycastHit hit;
        Debug.DrawLine(ray.origin, ray.origin + ray.direction * 500f, Color.red, 5);
        if (Physics.Raycast(ray, out hit, 500f, 1 << LayerMask.NameToLayer("Unit")))
        {
            currentlySelectedUnit = hit.transform.GetComponent<Unit>();
            currentlySelectedUnit.Select();
        }
        else
        {
            if (currentlySelectedUnit) currentlySelectedUnit.Deselect();
            currentlySelectedUnit = null;
        }
    }
}