using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Item")] [System.Serializable]
public class ItemData : ScriptableObject, ITutorialElementSpawnData
{
    public string displayName;

    public Buff buff;

    public GameObject modelPrefab;

    public Sprite icon;

    // Returns whether or not the use was succesful
    public virtual bool OnUse(Unit user)
    {
        return user.TryAddBuff(buff,buff.charges != -1);
    }

    public TutorialElementsSpawnData GetTutorialSpawnData()
    {
        return new TutorialElementsSpawnData() { itemData = this};
    }
}
