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
    public bool isImmuneToExplosions = false;
    public AIBehaviour aiBehaviour;
    public AttackType attackType;

    public SoundClip attackSound;
    public GameObject defaultPrefab;
}
