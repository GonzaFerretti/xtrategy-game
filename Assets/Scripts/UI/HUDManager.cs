using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class HUDManager : MonoBehaviour
{
    Dictionary<string,GameObject> HUDElements;
    [SerializeField] GameObject menu;
    [SerializeField] Transform switchableItemsPivot;
    [SerializeField] List<NamedButtonTrigger> namedButtonTriggers;
    [SerializeField] AdManager adManager;
    [SerializeField] GameObject adButton;
    public GameManager gm;

    [SerializeField] TutorialPrompt tutorialPromptPrefab;
    TutorialPrompt currentPrompt;

    public void Init()
    {
        InitElementList();
        gm = FindObjectOfType<GameManager>();
    }

    public void Awake()
    {
        InitNamedTriggers();
    }

    public void DisableAdButton()
    {
        if (adButton == null) return;
        HUDElements.Remove(adButton.name);
        GameObject.Destroy(adButton);
    }

    public void InitNamedTriggers()
    {
        PlayerController pc = FindObjectOfType<PlayerController>();

        foreach (var trigger in namedButtonTriggers)
        {
            trigger.SetPlayerController(pc);
        }
    }

    public void RestartLevel()
    {
        gm.saveManager.StageLoadForCleanLevel(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("Inicial");
    }

    public void SwitchMenu()
    {
        menu.SetActive(!menu.activeSelf);
    }

    public void SwitchAdMenu()
    {
        adManager.ToggleVisibility();
    }

    public void Save()
    {
        gm.SaveMatch();
    }

    public void EnableHudElementByName(string name)
    {
        if (HUDElements.ContainsKey(name)) HUDElements[name].SetActive(true);
    }

    public void DisableHudElementByName(string name)
    {
        if (HUDElements.ContainsKey(name))HUDElements[name].SetActive(false);
    }

    void InitElementList()
    {
        HUDElements = new Dictionary<string, GameObject>();
        for (int i = 0; i < switchableItemsPivot.childCount; i++)
        {
            GameObject HudElement = switchableItemsPivot.GetChild(i).gameObject;
            HUDElements.Add(HudElement.name, HudElement);
        }
    }

    public void DisableAllElements()
    {
        foreach (GameObject hudElement in HUDElements.Values)
        {
            hudElement.SetActive(false);
        }
    }

    public void ShowPrompt(TutorialStepInfoPromptConfig promptData)
    {
        currentPrompt = Instantiate(currentPrompt);
        (currentPrompt.transform as RectTransform).anchoredPosition = promptData.positionInScreen;
    }

    public void RemoveCurrentPrompt()
    {
        Destroy(currentPrompt.gameObject);
        currentPrompt = null;
    }
}
