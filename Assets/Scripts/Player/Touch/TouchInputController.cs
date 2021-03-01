using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchInputController : MonoBehaviour
{
    public delegate void TouchEventDelegate(Vector2 direction, float magnitude, TouchInputController controller);
    [SerializeField] TouchEvent currentTouchEvent;
    public Dictionary<string, TouchEventDelegate> eventTriggerLinks = new Dictionary<string, TouchEventDelegate>();
    [SerializeField] List<TouchAction> touchActions;
    [SerializeField] PlayerController playerController;

    bool areInteractionsLocked = false;

    public List<TouchEvent> possibleEvents = new List<TouchEvent>();

    public CameraController GetCameraController()
    {
        return playerController.GetGridReference().gameManager.camController;
    }

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

    public void UpdateInteractionLock(bool newStatus)
    {
        areInteractionsLocked = newStatus;
        EndCurrentEvent();
    }

    public void Start()
    {
        //playerController.GetGridReference().gameManager.OnInterfaceLock += UpdateInteractionLock;
        SubscribeAllTouchActions();
    }

    public void Update()
    {
        if (areInteractionsLocked) return;
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
