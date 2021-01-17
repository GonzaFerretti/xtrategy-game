using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    [SerializeField] float startingHeight;
    [SerializeField] float padding;
    [SerializeField] LevelSelectButton buttonPrefab;
    [SerializeField] SaveManager saveManager;
    [SerializeField] RectTransform backButton;

    void Start()
    {
        int count = 0;
        RectTransform rectTrans = GetComponent<RectTransform>();
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            string nextScene = SceneUtility.GetScenePathByBuildIndex(i);
            if (nextScene.Contains("Level"))
            {
                var newButton = Instantiate(buttonPrefab);
                newButton.GetComponent<RectTransform>().parent = rectTrans;
                string[] seperatedParts = nextScene.Replace(".unity", "").Split('/');
                string cleanSceneName = seperatedParts[seperatedParts.Length - 1];

                newButton.Setup(cleanSceneName, new Vector2(0,startingHeight - padding * count),saveManager);
                count++;
            }
        }

        float ySize = (count + 1) * padding + Mathf.Abs(startingHeight) * 2;
        rectTrans.sizeDelta = new Vector2(rectTrans.sizeDelta.x, ySize);

        backButton.anchoredPosition = new Vector2(0, startingHeight - padding * count);
    }
}
