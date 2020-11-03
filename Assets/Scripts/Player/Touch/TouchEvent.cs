using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

public class TouchEvent : ScriptableObject
{
    public string eName;
    protected int[] touchIds;
    protected Vector2[] touchesStartingPosition;
    protected TouchInputController controller;
    protected bool shouldEnd = false;
    

    public void SetInitialData(Touch[] touches, TouchInputController controller)
    {
        this.touchIds = new int[touches.Length];
        for (int i = 0; i < touches.Length; i++)
        {
            touchIds[i] = touches[i].fingerId;
        }
        this.controller = controller;
        touchesStartingPosition = new Vector2[touches.Length];
        for (int i = 0; i < touches.Length; i++)
        {
            Touch touch = touches[i];
            touchesStartingPosition[i] = touch.position;
        }
    }

    public virtual TouchEvent CreateInstance()
    {
        TouchEvent touchEvent = Instantiate(this);
        touchEvent.eName = eName;
        return touchEvent;
    }

    public bool CheckEnded()
    {
        return shouldEnd || CheckConditionForEventEnd();
    }

    public virtual bool CheckConditionForEventEnd()
    {
        return false;
    }

    protected bool GetTouchById(out Touch touch, int id)
    {
        foreach (Touch possibleTouch in Input.touches)
        {
            if (possibleTouch.fingerId == id)
            {
                touch = possibleTouch;
                return true;
            }
        }

        touch = new Touch();
        return false;
    }

    public virtual bool CheckConditionForEventStart()
    {
        return false;
    }

    public virtual void OnTrigger()
    { 
    }
}
