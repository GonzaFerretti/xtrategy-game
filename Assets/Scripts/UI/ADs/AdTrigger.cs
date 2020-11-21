using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdTrigger : MonoBehaviour
{
    [SerializeField] AdType adType;
    [SerializeField] AdManager admgr;

    public void ActivateAdTrigger()
    {
        admgr.ShowAd(adType.ToString());
    }
}
