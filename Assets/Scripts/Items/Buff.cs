using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Buff")]
public class Buff : ScriptableObject
{
    public string identifier;
    public Sprite icon; 
    public int charges = -1;
}
