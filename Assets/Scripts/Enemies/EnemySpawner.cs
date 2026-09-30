using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private WaveManager waveManager;

    [Header("Spawn Distance")]
    [SerializeField] private float spawnDistance = 8f;

    [Header("Spawn Rate")]
    [SerializeField] private float startingSpawnInterval = 2f;
    [SerializeField] private float endingSpawnInterval = 0.5f;

    [Header("Enemy Limit")]
    [SerializeField] private int baseMaxEnemies = 20;

    private float spawnTimer;

    private void Update()
    {
        if (waveManager == null || !waveManager.WaveActive)
            return;

        if (player == null)
            return;

        spawnTimer -= Time.deltaTime;

        float currentSpawnInterval = GetCurrentSpawnInterval();

        if (spawnTimer <= 0f)
        {
            SpawnEnemy();
            spawnTimer = currentSpawnInterval;
        }
    }

    private float GetCurrentSpawnInterval()
    {
        WaveData currentWave = waveManager.CurrentWaveData;

        if (currentWave == null)
            return startingSpawnInterval;

        float progress =
            1f - (waveManager.WaveTimer / currentWave.duration);

        float interval = Mathf.Lerp(
            startingSpawnInterval,
            endingSpawnInterval,
            progress
        );

        float difficulty =
            waveManager.GetDifficultyMultiplier();

        return interval / difficulty;
    }

    private void SpawnEnemy()
    {
        GameObject[] existingEnemies =
            GameObject.FindGameObjectsWithTag("Enemy");

        int maxEnemies = Mathf.RoundToInt(
            baseMaxEnemies *
            waveManager.GetDifficultyMultiplier()
        );

        if (existingEnemies.Length >= maxEnemies)
            return;

        GameObject selectedPrefab = GetRandomEnemy();

        if (selectedPrefab == null)
            return;

        Vector2 randomDirection =
            Random.insideUnitCircle.normalized;

        Vector3 spawnPosition =
            player.position +
            new Vector3(
                randomDirection.x,
                randomDirection.y,
                0f
            ) * spawnDistance;

        GameObject enemy = Instantiate(
            selectedPrefab,
            spawnPosition,
            Quaternion.identity
        );

        EnemyBase enemyBase =
            enemy.GetComponent<EnemyBase>();

        if (enemyBase != null)
        {
            enemyBase.moveSpeed *=
                waveManager.GetDifficultyMultiplier();
        }
    }

    private GameObject GetRandomEnemy()
    {
        WaveData currentWave = waveManager.CurrentWaveData;

        if (currentWave == null ||
            currentWave.enemies == null ||
            currentWave.enemies.Length == 0)
        {
            return null;
        }

        float totalChance = 0f;

        foreach (EnemySpawnData enemy in currentWave.enemies)
        {
            if (enemy.enemyPrefab != null)
            {
                totalChance += enemy.spawnChance;
            }
        }

        if (totalChance <= 0f)
            return null;

        float randomValue =
            Random.Range(0f, totalChance);

        float currentChance = 0f;

        foreach (EnemySpawnData enemy in currentWave.enemies)
        {
            if (enemy.enemyPrefab == null)
                continue;

            currentChance += enemy.spawnChance;

            if (randomValue <= currentChance)
            {
                return enemy.enemyPrefab;
            }
        }

        return null;
    }
}