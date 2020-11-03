using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchInputController : MonoBehaviour
{
    public delegate void TouchEventDelegate(Vector2 direction, float magnitude);
    [SerializeField] TouchEvent currentTouchEvent;
    public Dictionary<string, TouchEventDelegate> eventTriggerLinks = new Dictionary<string, TouchEventDelegate>();
    [SerializeField] List<TouchAction> touchActions;

    public List<TouchEvent> possibleEvents = new List<TouchEvent>();

    public void SubscribeToTouchEvent(TouchAction actionToRegister)
    {
        if (!eventTriggerLinks.ContainsKey(actionToRegister.triggerEvent.eName))
        {
            eventTriggerLinks.Add(actionToRegister.triggerEvent.eName, actionToRegister.Action);
        }
        else
        {
            // Following some StackOverflow stuff, removing the void first won't return an error and will make sure there are not duplicates.
            eventTriggerLinks[actionToRegister.triggerEvent.eName] -= actionToRegister.Action;
            eventTriggerLinks[actionToRegister.triggerEvent.eName] += actionToRegister.Action;
        }
    }

    public void SubscribeAllTouchActions()
    {
        foreach (TouchAction touchAction in touchActions)
        {
            SubscribeToTouchEvent(touchAction);
        }
    }

    public void Start()
    {
        SubscribeAllTouchActions();
    }

    public void Update()
    {
        if (currentTouchEvent == null)
        {
            if (Input.touchCount > 0 && Input.touchCount <= 2)
            {
                bool isMoving = true;
                foreach (Touch touch in Input.touches)
                {
                    isMoving = isMoving && touch.phase == TouchPhase.Moved;
                }
                if (isMoving)
                {
                    foreach (TouchEvent possibleEvent in possibleEvents)
                    {
                        if (possibleEvent.CheckConditionForEventStart())
                        {
                            currentTouchEvent = possibleEvent.CreateInstance();
                            currentTouchEvent.SetInitialData(Input.touches, this);
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            if (currentTouchEvent.CheckEnded())
            {
                EndCurrentEvent();
            }
            else
            {
                currentTouchEvent.OnTrigger();
            }
        }
    }

    public void EndCurrentEvent()
    {
        currentTouchEvent = null;
    }


}
