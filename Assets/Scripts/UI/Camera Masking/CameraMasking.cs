using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMasking : MonoBehaviour
{
    //public List<GameObject> lastMaskedBuildings = new List<GameObject>();
    public float checkDistanceDelta;
    public float perpendicularLength;
    public int buildingLayerId;
    private WideCamMaskCheck wideCheckScript;
    public PlayerController player;
    public float timeAfterFirstHide;

    public BoxCollider longCollider;
    public BoxCollider wideCollider;

    private void Start()
    {
        longCollider = GetComponent<BoxCollider>();
        wideCheckScript = wideCollider.GetComponent<WideCamMaskCheck>();
        wideCheckScript.timeAfterHide = timeAfterFirstHide;
        wideCheckScript.buildingLayerId = buildingLayerId;
        longCollider.center = new Vector3(0, 0, checkDistanceDelta / 2);
        StartCoroutine(FindPlayerWithDelay());
    }

    IEnumerator FindPlayerWithDelay()
    {
        yield return null; //new WaitUntil(() => FindObjectOfType<ModelPlayable>() != null);
        //player = FindObjectOfType<ModelPlayable>();
    }

    void Update()
    {
        if (player != null)
        { 
        UpdateCheckDistance();
        }
        //CheckCoveringBuilding();
    }

    void UpdateCheckDistance()
    {
        float newDistance = (transform.position - player.transform.position).magnitude;
        longCollider.size = new Vector3(longCollider.size.x, longCollider.size.y, newDistance - checkDistanceDelta);
        longCollider.center = new Vector3(0, 0, (newDistance -checkDistanceDelta) / 2f);
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == buildingLayerId)
        {
            other.GetComponent<Building>().Hide(timeAfterFirstHide);
            wideCollider.enabled = true;
            wideCollider.size = new Vector3(perpendicularLength, 0.25f, 0.25f);
            wideCollider.transform.position = other.transform.position;
            wideCollider.transform.right = other.GetComponent<BuildingMaskSettings>().maskDirection;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == buildingLayerId)
        {
            wideCollider.enabled = false;
        }
    }
}
