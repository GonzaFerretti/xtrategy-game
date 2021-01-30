using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class ItemData : ScriptableObject
{
    public string displayName;

    public ItemVisualInfo itemVisualInfo;

    public Sprite icon;

    // Returns whether or not the use was succesful
    public abstract bool OnUse(Unit user);
}
