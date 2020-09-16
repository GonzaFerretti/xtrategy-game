using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseController : MonoBehaviour
{
    [SerializeField]protected Unit currentlySelectedUnit;
    [SerializeField] protected GameGridManager gridManager;
}
