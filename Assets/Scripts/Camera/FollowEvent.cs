using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowEvent
{
    public bool shouldStop;
    public Transform target;
    public FollowEvent(Transform target)
    {
        shouldStop = false;
        this.target = target;
    }
}
