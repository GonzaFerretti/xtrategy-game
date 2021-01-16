using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] Image fillLoadImage;
    bool isLoadingNewLevel = false;

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
            UpdateBar(loadHandle.progress/2);
            yield return null;
        }

        loadHandle.allowSceneActivation = true;

        while (!loadHandle.isDone)
        {
            yield return null;
        }

        FindObjectOfType<GameManager>().InitiateGame();
        // TODO Here we could add a fade transition for the level
        Destroy(gameObject);
    }
}
