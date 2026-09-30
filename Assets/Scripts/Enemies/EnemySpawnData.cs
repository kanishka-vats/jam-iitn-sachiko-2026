using UnityEngine;

[System.Serializable]
public class EnemySpawnData
{
    public GameObject enemyPrefab;

    [Range(0f, 100f)]
    public float spawnChance = 100f;
}