using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Touch Input/Event/Swipe")]
public class TouchEventSwipe : TouchEvent
{
    public override bool CheckConditionForEventStart()
    {
        return Input.touchCount == 1;
    }

    public override void OnTrigger()
    {
        if (GetTouchById(out Touch firstTouch, touchIds[0]))
        {
            if (controller.eventTriggerLinks.ContainsKey(eName))
            {
                controller.eventTriggerLinks[eName](firstTouch.deltaPosition, 0,controller);
            }
        }
        else
        {
            shouldEnd = true;
        }
    }
}
