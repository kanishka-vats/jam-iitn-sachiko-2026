using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyPoolData
{
    public string poolName; 
    public GameObject enemyPrefab;
    public int poolSize = 10;
    
    
    [HideInInspector] 
    public Queue<GameObject> pool = new Queue<GameObject>(); 
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public EnemyPoolData[] enemyPools; 
    public Transform[] spawnPoints;
    public float spawnInterval = 1.5f;

    void Start()
    {
        InitializePools();
    }

    private void InitializePools()
    {
        foreach (var poolData in enemyPools)
        {
            for (int i = 0; i < poolData.poolSize; i++)
            {
                GameObject enemy = Instantiate(poolData.enemyPrefab, transform);
                enemy.SetActive(false);
                poolData.pool.Enqueue(enemy);
            }
        }
    }

    public void StartSpawning(int numberOfEnemies)
    {
        StartCoroutine(SpawnRoutine(numberOfEnemies));
    }

    private IEnumerator SpawnRoutine(int enemiesToSpawn)
    {
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnRandomEnemyType();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnRandomEnemyType()
    {
        
        int randomPoolIndex = Random.Range(0, enemyPools.Length);
        EnemyPoolData selectedPool = enemyPools[randomPoolIndex];

        if (selectedPool.pool.Count == 0) return; 

        GameObject enemyToSpawn = selectedPool.pool.Dequeue();
        Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        enemyToSpawn.transform.position = randomPoint.position;
        enemyToSpawn.SetActive(true);
        
        selectedPool.pool.Enqueue(enemyToSpawn); 
    }
}