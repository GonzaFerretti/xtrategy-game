using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WideCamMaskCheck : MonoBehaviour
{
    //public List<Collider> hiddenObjects = new List<Collider>();
    public int buildingLayerId;
    public float timeAfterHide;

    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == buildingLayerId)
        {
            other.GetComponent<Building>().Hide(timeAfterHide);
            /*if (!hiddenObjects.Contains(other))
            {
                hiddenObjects.Add(other);
            }*/
        }
    }

    public void SwitchPosition()
    {
        /*
        Debug.Log("switching");
        foreach(Collider col in hiddenObjects)
        {
            //col.GetComponent<Building>().UnHide(timeAfterHide);
        }
        hiddenObjects = new List<Collider>();*/
    }
}
