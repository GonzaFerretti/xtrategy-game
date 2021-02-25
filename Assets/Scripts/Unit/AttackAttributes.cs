using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/AttackAttributes")]
[System.Serializable]
public class AttackAttributes : ScriptableObject
{
    public int minAttackRange;
    public int maxAttackRange;
    public int damage;
    public int damageBoost;
    public AttackType attackType;
    public SoundClip attackSound;
}
