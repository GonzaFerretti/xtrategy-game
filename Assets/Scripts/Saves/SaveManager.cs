using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class SaveManager : MonoBehaviour
{
    string basePath;
    public bool isLoadingFromSave = false;

    private void Start()
    {
        foreach (var saveManagerInstance in FindObjectsOfType<SaveManager>())
        {
            if (saveManagerInstance != this) Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        basePath = Application.streamingAssetsPath;
    }

    public void ProcessDataAndSave(string levelName, List<Unit> units, bool hasUsedPower, bool isEnemyTurn)
    {
        var saveData = new SaveData
        {
            levelName = levelName,
            units = GetSaveInfoFromUnits(units),
            hasUsedPower = hasUsedPower,
            isEnemyTurn = isEnemyTurn
        };

        Save(saveData);
    }

    public UnitSaveInfo[] GetSaveInfoFromUnits(List<Unit> units)
    {
        UnitSaveInfo[] saveInfo = new UnitSaveInfo[units.Count];
        for (int i = 0; i < units.Count; i++)
        {
            Unit unit = units[i];
            if (!unit) continue;
            saveInfo[i] = new UnitSaveInfo
            {
                hasAttacked = unit.attackState == CurrentActionState.ended,
                hasMoved = unit.moveState == CurrentActionState.ended,
                hpLeft = unit.currentHp,
                owner = unit.owner.name,
                unitType = unit.unitAttributes,
                position = unit.GetCoordinates()
            };
        }
        return saveInfo;
    }

    public void Save(SaveData saveData)
    {
        DeleteExistingSave();
        var file = File.CreateText(GetCurrentTimeFullPath());
        string json = JsonUtility.ToJson(saveData, true);
        file.Write(json);
        file.Close();
    }

    public SaveData Load()
    {
        DirectoryInfo di = new DirectoryInfo(basePath);
        FileInfo[] savesInFolder = di.GetFiles("*.json");
        if (savesInFolder.Length == 0) return null;

        string saveDataRaw = File.ReadAllText(savesInFolder[0].FullName);
        return JsonUtility.FromJson<SaveData>(saveDataRaw);
    }

    public void DeleteExistingSave()
    {
        DirectoryInfo di = new DirectoryInfo(basePath);
        FileInfo[] savesInFolder = di.GetFiles("*.json");
        foreach (var saveFile in savesInFolder)
        {
            saveFile.Delete();
        }
    }

    public string GetCurrentTimeFullPath()
    {
        string currentTime = System.DateTime.Now.ToString("yyyyMMdd-HH_mm_ss");
        return basePath + "/" + currentTime + ".json";
    }
}
