using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NamedButtonTrigger : EventTrigger
{
    protected PlayerController playerController;
    [SerializeField] protected string buttonName;

    public virtual void SetPlayerController(PlayerController pc)
    {
        playerController = pc;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        playerController.OnButtonPressUp(buttonName);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        playerController.OnButtonPressDown(buttonName);
    }
}
