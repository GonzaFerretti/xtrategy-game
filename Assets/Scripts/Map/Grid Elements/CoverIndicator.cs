using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverIndicator : MonoBehaviour
{
    [SerializeField] GameObject side1;
    [SerializeField] GameObject side2;

    public void SetDistance(float distance)
    {
        side1.transform.localPosition = -distance * Vector3.forward;
        side2.transform.localPosition = distance * Vector3.forward;
    }
}
