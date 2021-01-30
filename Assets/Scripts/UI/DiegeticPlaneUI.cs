using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiegeticPlaneUI : MonoBehaviour
{
    public virtual void Update()
    {
        transform.forward = Camera.main.transform.forward;
    }
}
