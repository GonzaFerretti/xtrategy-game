using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchingIcon : MonoBehaviour
{
    [SerializeField] Sprite attackIcon;
    [SerializeField] Sprite walkIcon;
    [SerializeField] Image img;
    public void OnSwitchIcon()
    {
        if (img.sprite == attackIcon)
        {
            img.sprite = walkIcon;
        }
        else
        {
            img.sprite = attackIcon;
        }
    }
}
