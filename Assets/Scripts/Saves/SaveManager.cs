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
    [SerializeField] string loadingSceneName;
    public SaveData stagedSaveDataToLoad;
    public UnitTypeBank unitTypeBank;
    public ItemTypeBank itemTypeBank;
    public BuffTypeBank buffTypeBank;

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

    public void ProcessDataAndSave(PreSaveData data)
    {
        var saveData = new SaveData
        {
            levelName = data.levelName,
            units = GetSaveInfoFromUnits(data.units),
            mines = GetMineSaveInfo(data.mines),
            pickupItems = GetItemSaveInfo(data.pickupItems),
            hasUsedPower = data.hasUsedPower,
            isEnemyTurn = data.isEnemyTurn,
            currentPlayerItemId = itemTypeBank.GetItemId(data.playerCurrentItem)
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

    public PickupItemSaveInfo[] GetItemSaveInfo(List<ItemPickup> items)
    {
        PickupItemSaveInfo[] saveInfo = new PickupItemSaveInfo[items.Count];
        for (int i = 0; i < items.Count; i++)
        {
            ItemPickup item = items[i];
            if (!item) continue;
            saveInfo[i] = new PickupItemSaveInfo
            {
                position = item.coordinates,
                itemId = itemTypeBank.GetItemId(item.itemData)
            };
        }
        return saveInfo;
    }

    public SavedBuffInfo[] GetSavedBuffInfoFromUnit(Unit unit)
    {
        List<Buff> buffs = unit.GetCurrentlyActiveBuffs();
        SavedBuffInfo[] saveInfo = new SavedBuffInfo[buffs.Count];
        for (int i = 0; i < buffs.Count; i++)
        {
            Buff buff = buffs[i];
            if (!buff) continue;
            saveInfo[i] = new SavedBuffInfo
            {
                identifier = buff.identifier,
                remainingCharges = buff.charges
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
                unitId = unitTypeBank.GetUnitId(unit.unitAttributes),
                position = unit.GetCoordinates(),
                activeBuffs = GetSavedBuffInfoFromUnit(unit)
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

    public void StageLoadFromSave()
    {
        DirectoryInfo di = new DirectoryInfo(basePath);
        FileInfo[] savesInFolder = di.GetFiles("*.json");
        if (savesInFolder.Length == 0) return;

        isLoadingFromSave = true;
        string saveDataRaw = File.ReadAllText(savesInFolder[0].FullName);
        stagedSaveDataToLoad = JsonUtility.FromJson<SaveData>(saveDataRaw);

        AsyncOperation loadHandle = SceneManager.LoadSceneAsync(loadingSceneName);
        StartCoroutine(WaitForLoadingSceneLoad(loadHandle));
    }

    public void StageLoadForCleanLevel(string levelToLoadEmpty)
    {
        stagedSaveDataToLoad = new SaveData
        {
            levelName = levelToLoadEmpty
        };

        AsyncOperation loadHandle = SceneManager.LoadSceneAsync(loadingSceneName);
        StartCoroutine(WaitForLoadingSceneLoad(loadHandle));
    }

    IEnumerator WaitForLoadingSceneLoad(AsyncOperation loadHandle)
    {
        while (!loadHandle.isDone)
        {
            yield return null;
        }

        LoadingScreen loadingScreen = FindObjectOfType<LoadingScreen>();

        if (!loadingScreen)
        {
            throw new ArgumentException("Couldn't find loading screen");
        }

        loadingScreen.StartLevelAsyncLoad(stagedSaveDataToLoad);
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

[System.Serializable]
public struct UnitTypeBank
{
    [SerializeField] List<UnitAttributes> unitTypes;

    public UnitAttributes GetUnitType(int id)
    {
        if (id <= unitTypes.Count-1 && id >= 0)
        {
            return unitTypes[id];
        }
        return null;
    }

    public int GetUnitId(UnitAttributes unitType)
    {
        if (unitTypes.Contains(unitType))
        {
            return unitTypes.IndexOf(unitType);
        }
        else return -1;
    }
}

[System.Serializable]
public struct ItemTypeBank
{
    [SerializeField] List<ItemData> itemTypes;

    public ItemData GetItemType(int id)
    {
        if (id <= itemTypes.Count - 1 && id >= 0)
        {
            return itemTypes[id];
        }
        return null;
    }

    public int GetItemId(ItemData itemType)
    {
        if (itemTypes.Contains(itemType))
        {
            return itemTypes.IndexOf(itemType);
        }
        else return -1;
    }
}

[System.Serializable]
public struct BuffTypeBank
{
    [SerializeField] List<Buff> buffTypes;

    public Buff GetBuffType(string identifier)
    {
        foreach (var buff in buffTypes)
        {
            if (buff.identifier == identifier) return buff;
        }
        return null;
    }
}

public struct PreSaveData
{
    public string levelName;
    public List<Unit> units;
    public bool hasUsedPower;
    public bool isEnemyTurn;
    public List<MagicMine> mines;
    public List<ItemPickup> pickupItems;
    public ItemData playerCurrentItem;
}

