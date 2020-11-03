using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Touch Input/Event/Pinch")]
public class TouchEventPinch : TouchEvent
{
    float lastMagnitude = float.NaN;
    public override bool CheckConditionForEventStart()
    {
        return Input.touchCount == 2;
    }

    public override void OnTrigger()
    {
        if (GetTouchById(out Touch firstTouch, touchIds[0]) && GetTouchById(out Touch secondTouch, touchIds[1]))
        {
            if (controller.eventTriggerLinks.ContainsKey(eName))
            {
                if (float.IsNaN(lastMagnitude))
                {
                    lastMagnitude = (firstTouch.position - secondTouch.position).magnitude;
                }
                else
                {
                    float dist = lastMagnitude - (firstTouch.position - secondTouch.position).magnitude;
                    controller.eventTriggerLinks[eName](Vector2.zero, dist);
                    lastMagnitude = (firstTouch.position - secondTouch.position).magnitude;
                }
            }
        }
        else
        {
            shouldEnd = true;
        }
    }
}
