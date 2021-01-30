using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemButtonTrigger : NamedButtonTrigger
{
    [SerializeField] Image image;
    [SerializeField] GameObject container;

    public void ChangeIcon(Sprite itemIcon)
    {
        image.sprite = itemIcon;
    }

    public override void SetPlayerController(PlayerController pc)
    {
        base.SetPlayerController(pc);
        pc.SetItemButtonReference(this);
    }

    public void SetUsability(bool canBeUsed)
    {
        container.SetActive(canBeUsed);
    }
}
