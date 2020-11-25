using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingHeal : MonoBehaviour
{
    [SerializeField] float onScreenTime;
    [SerializeField] Image icon;

    public IEnumerator ShowIcon()
    {
        icon.enabled = true;

        yield return new WaitForSeconds(onScreenTime);

        icon.enabled = false;
    }
}
