using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    string basePath;
    public bool isLoadingFromSave = false;
    public SaveData stagedSaveDataToLoad;

    public void ResetStagedData()
    {
        isLoadingFromSave = false;
        stagedSaveDataToLoad = null;
    }

    private void Start()
    {
        foreach (var saveManagerInstance in FindObjectsOfType<SaveManager>())
        {
            if (saveManagerInstance != this)
            {
                if (SceneManager.GetActiveScene().name == "Inicial")
                {
                    Destroy(saveManagerInstance.gameObject);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
        DontDestroyOnLoad(gameObject);
        basePath = Application.persistentDataPath;
    }

    public void ProcessDataAndSave(string levelName, List<Unit> units, bool hasUsedPower, bool isEnemyTurn, List<MagicMine> mines)
    {
        var saveData = new SaveData
        {
            levelName = levelName,
            units = GetSaveInfoFromUnits(units),
            mines = GetMineSaveInfo(mines),
            hasUsedPower = hasUsedPower,
            isEnemyTurn = isEnemyTurn
        };

        Save(saveData);
    }

    public MineSaveInfo[] GetMineSaveInfo(List<MagicMine> mines)
    {
        MineSaveInfo[] saveInfo = new MineSaveInfo[mines.Count];
        for (int i = 0; i < mines.Count; i++)
        {
            MagicMine mine = mines[i];
            if (!mine) continue;
            saveInfo[i] = new MineSaveInfo
            {
                owner = mine.owner.name,
                position = mine.coordinates,
                detonationTiles = mine.detonationTiles,
                triggerTiles = mine.triggerTiles
            };
        }
        return saveInfo;
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
                position = unit.GetCoordinates(),
                isShielded = unit.isShielded
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

    public void StageLoad()
    {
        DirectoryInfo di = new DirectoryInfo(basePath);
        FileInfo[] savesInFolder = di.GetFiles("*.json");
        if (savesInFolder.Length == 0) return;

        string saveDataRaw = File.ReadAllText(savesInFolder[0].FullName);
        stagedSaveDataToLoad = JsonUtility.FromJson<SaveData>(saveDataRaw);

        isLoadingFromSave = true;
        SceneManager.LoadScene(stagedSaveDataToLoad.levelName);
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
