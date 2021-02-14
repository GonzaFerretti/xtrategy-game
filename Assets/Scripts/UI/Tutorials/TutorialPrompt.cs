using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialPrompt : MonoBehaviour
{
    List<TutorialPromptArrow> arrows;
    [SerializeField] TextMeshProUGUI text;

    public void Init(string textToShow, TSPromptArrowDirection arrowDirection)
    {
        text.SetText(textToShow);
        foreach (var arrow in arrows)
        {
            if (arrow.direction == arrowDirection)
            {
                arrow.enabled = true;
                break;
            }
        }
    }
}
