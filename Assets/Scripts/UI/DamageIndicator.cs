using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DamageIndicator : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI amount;
    [SerializeField] Animator anim;

    [SerializeField] Color[] damageTypeColors;

    public void ShowDamage(int damage, DamageType damageType)
    {
        amount.SetText("-" + damage.ToString());

        amount.color = damageTypeColors[(int)damageType];

        anim.Play("damageIndicatorFadeOut");
    }
}

public enum DamageType
{
    reduced,
    normal,
    boosted,
}
