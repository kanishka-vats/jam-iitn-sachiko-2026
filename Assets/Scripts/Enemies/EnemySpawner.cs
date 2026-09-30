using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject walkerPrefab;
    public Transform player;
    public WaveManager waveManager;

    [Header("Spawn Distance")]
    public float spawnDistance = 8f;

    [Header("Spawn Rate")]
    public float startingSpawnInterval = 2f;
    public float endingSpawnInterval = 0.5f;

    [Header("Enemy Limit")]
    public int baseMaxEnemies = 20;

    private float spawnTimer;

    private void Update()
    {
        if (!waveManager.WaveActive)
            return;

        spawnTimer -= Time.deltaTime;

        float currentSpawnInterval = GetCurrentSpawnInterval();

        if (spawnTimer <= 0f)
        {
            SpawnWalker();
            spawnTimer = currentSpawnInterval;
        }
    }

    private float GetCurrentSpawnInterval()
    {
        float progress = 1f - 
            (waveManager.WaveTimer / waveManager.waveDuration);

        float interval = Mathf.Lerp(
            startingSpawnInterval,
            endingSpawnInterval,
            progress
        );

        float difficulty = waveManager.GetDifficultyMultiplier();

        return interval / difficulty;
    }

    private void SpawnWalker()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        int maxEnemies = Mathf.RoundToInt(
            baseMaxEnemies * waveManager.GetDifficultyMultiplier()
        );

        if (enemies.Length >= maxEnemies)
            return;

        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        Vector3 spawnPosition =
            player.position +
            new Vector3(randomDirection.x, randomDirection.y, 0f) *
            spawnDistance;

        GameObject enemy = Instantiate(
            walkerPrefab,
            spawnPosition,
            Quaternion.identity
        );

        EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();

        if (enemyBase != null)
        {
            enemyBase.moveSpeed *= waveManager.GetDifficultyMultiplier();
        }
    }
}