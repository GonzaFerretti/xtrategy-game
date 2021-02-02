using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuffIcon : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] TextMeshProUGUI amountText;

    public void Setup(Sprite icon, int time)
    {
        image.sprite = icon;
        amountText.gameObject.SetActive(time > 0);
        amountText.text = time.ToString();
    }

    public void UpdateAmount(int amount)
    {
        amountText.text = amount.ToString();
    }
}
