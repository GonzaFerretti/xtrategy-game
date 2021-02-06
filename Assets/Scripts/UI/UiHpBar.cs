using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiHpBar : DiegeticPlaneUI
{
    public Image fillBar;

    public void UpdateHPbar(float currentPercentage)
    {
        fillBar.fillAmount = currentPercentage;
    }
}
