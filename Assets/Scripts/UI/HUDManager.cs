using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HUDManager : MonoBehaviour
{
    Dictionary<string,GameObject> HUDElements;
    [SerializeField] GameObject menu;

    public void Start()
    {
        InitElementList();
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene("Level 1");
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("Inicial");
    }

    public void SwitchMenu()
    {
        menu.SetActive(!menu.activeSelf);
    }

    public void EnableHudElementByName(string name)
    {
        HUDElements[name].SetActive(true);
    }

    public void DisableHudElementByName(string name)
    {
        HUDElements[name].SetActive(false);
    }

    void InitElementList()
    {
        HUDElements = new Dictionary<string, GameObject>();
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject HudElement = transform.GetChild(i).gameObject;
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
}
