using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class BuildingMaskSettings : MonoBehaviour
{
    public Vector3 maskDirection;

#if UNITY_EDITOR
    private void Update()
    {
        Debug.DrawLine(transform.position - maskDirection / 2, transform.position + maskDirection / 2, Color.cyan, Time.deltaTime);
    }
#endif
}
