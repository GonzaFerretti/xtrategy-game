using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectButton : MonoBehaviour
{
    [SerializeField] Text text;
    [SerializeField] Button button;
    
    public void Setup(string levelName, Vector2 position, SaveManager saveManager)
    {
        text.text = levelName;
        gameObject.name = levelName;
        RectTransform trans = GetComponent<RectTransform>();
        trans.anchoredPosition = position;
        trans.localScale = new Vector2(1, 1);
        button.onClick.AddListener(() => saveManager.StageLoadForCleanLevel(levelName));
    }
}
