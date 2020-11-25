using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiHpBar : MonoBehaviour
{
    public Image fillBar;

    
    public void UpdateHPbar(float currentPercentage)
    {
        fillBar.fillAmount = currentPercentage;
    }

    public void Update()
    {
        transform.forward = Camera.main.transform.forward;
    }
}
