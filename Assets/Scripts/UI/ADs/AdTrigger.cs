using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdTrigger : MonoBehaviour
{
    [SerializeField] string adType;
    [SerializeField] AdManager admgr;

    public void ActivateAdTrigger()
    {
        admgr.ShowAd(adType.ToString());
    }
}
