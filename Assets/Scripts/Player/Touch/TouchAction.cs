using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TouchAction : ScriptableObject
{
    public TouchEvent triggerEvent;
    public PlayerController player;
    public abstract void Action(Vector2 direction, float magnitude);
}
