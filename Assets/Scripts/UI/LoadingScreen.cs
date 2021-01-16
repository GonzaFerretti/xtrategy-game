using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] Image fillLoadImage;
    [SerializeField] float baseLevelLoadShownPercentage;
    [HideInInspector] public float inLevelSetupProgress = 0;

    void UpdateBar(float percentage)
    {
        fillLoadImage.fillAmount = percentage;
    }

    public void StartLevelAsyncLoad(SaveData dataToLoad)
    {
        AsyncOperation loadHandle = SceneManager.LoadSceneAsync(dataToLoad.levelName);
        DontDestroyOnLoad(gameObject);
        loadHandle.allowSceneActivation = false;
        StartCoroutine(WaitForSceneLoad(loadHandle, dataToLoad));
    }

    IEnumerator WaitForSceneLoad(AsyncOperation loadHandle, SaveData dataToLoad)
    {
        while (loadHandle.progress < 0.9f)
        {
            Debug.Log(loadHandle.progress);
            UpdateBar(loadHandle.progress*0.9f*(1-baseLevelLoadShownPercentage));
            yield return null;
        }

        loadHandle.allowSceneActivation = true;

        while (!loadHandle.isDone)
        {
            yield return null;
        }

        GameManager gm = null;

        do
        {
            gm = FindObjectOfType<GameManager>();
            yield return null;
        } while (!gm);

        StartCoroutine(gm.InitiateGame(this));
        while (inLevelSetupProgress < 1)
        {
            UpdateBar(baseLevelLoadShownPercentage + inLevelSetupProgress * (1-baseLevelLoadShownPercentage));
            yield return null;
        }
        // TODO Here we could add a fade transition for the level
        Destroy(gameObject);
    }
}
