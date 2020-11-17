using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/UnitAttributes")] [System.Serializable]
public class UnitAttributes : ScriptableObject
{
    public int maxHp;
    public int movementRange;
    public int minAttackRange;
    public int maxAttackRange;
    public int damage;
    public AIBehaviour aiBehaviour;

    public SoundClip attackSound;
    public GameObject defaultPrefab;
}
