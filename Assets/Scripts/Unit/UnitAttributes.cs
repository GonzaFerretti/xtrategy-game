using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/UnitAttributes")] [System.Serializable]
public class UnitAttributes : ScriptableObject, ITutorialElementSpawnData
{
    public int maxHp;
    public int movementRange;
    public int movementBoost;
    public bool isImmuneToExplosives = false;
    public AIBehaviour aiBehaviour;
    public AttackAttributes mainAttack;
    public GameObject defaultPrefab;

    public TutorialElementsSpawnData GetTutorialSpawnData()
    {
        return new TutorialElementsSpawnData() { unitData = this };
    }
}
