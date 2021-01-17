using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    Dictionary<string, GameObject> possibleMenus;

    [SerializeField] GameObject initialMenu;

    public void LoadMenuDict()
    {
        possibleMenus = new Dictionary<string, GameObject>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform childTrans = transform.GetChild(i);
            possibleMenus.Add(childTrans.name, childTrans.gameObject);
        }
    }

    public void LoadMenu(string name)
    {
        if (!possibleMenus.ContainsKey(name)) return;

        foreach (var menu in possibleMenus)
        {
            menu.Value.SetActive(false);
        }

        possibleMenus[name].SetActive(true);
    }

    public void Start()
    {
        LoadMenuDict();
        LoadMenu(initialMenu.name);
    }
}
