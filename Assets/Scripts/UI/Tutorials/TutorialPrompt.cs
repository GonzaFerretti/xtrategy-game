using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialPrompt : MonoBehaviour
{
    [SerializeField] List<TutorialPromptArrow> arrows;
    [SerializeField] TextMeshProUGUI text;

    public void Init(TutorialStepInfoPromptConfig promptData)
    {
        text.SetText(promptData.promptText);
        if (promptData.arrowDirection == TSPromptArrowDirection.none) return;
        foreach (var arrow in arrows)
        {
            if (arrow.direction == promptData.arrowDirection)
            {
                arrow.gameObject.SetActive(true);
                break;
            }
        }
    }
}
