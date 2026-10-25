
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

    [Header("Formation Settings")]
    [SerializeField] private int formationEnemyCount = 6;
    [SerializeField] private float circleRadius = 3f;
    [SerializeField] private float zigZagSpacing = 1.5f;
    [SerializeField] private float zigZagWidth = 5f;
    [SerializeField] private float formationMovementDelay = 1f;

    private float spawnTimer;
    private int formationIndex;

    private enum SpawnPattern
    {
        Circle,
        ZigZag
    }

    private void Update()
    {
        if (waveManager == null || !waveManager.WaveActive || player == null)
            return;

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            if (waveManager.CurrentWave <= 2)
                SpawnRandomEnemy();
            else
                SpawnFormation();

            spawnTimer = GetCurrentSpawnInterval();
        }
    }

    private float GetCurrentSpawnInterval()
    {
        WaveData currentWave = waveManager.CurrentWaveData;

        if (currentWave == null)
            return startingSpawnInterval;

        float progress = currentWave.duration > 0f
            ? 1f - Mathf.Clamp01(waveManager.WaveTimer / currentWave.duration)
            : 1f;

        float interval = Mathf.Lerp(
            startingSpawnInterval,
            endingSpawnInterval,
            progress
        );

        return interval / Mathf.Max(0.01f, waveManager.GetDifficultyMultiplier());
    }

    private void SpawnRandomEnemy()
    {
        if (!HasEnemyCapacity())
            return;

        Vector2 direction = Random.insideUnitCircle.normalized;

        if (direction.sqrMagnitude < 0.01f)
            direction = Vector2.right;

        Vector3 position = player.position + (Vector3)(direction * spawnDistance);

        SpawnEnemyAt(position, false);
    }

    private void SpawnFormation()
    {
        SpawnPattern pattern = formationIndex % 2 == 0
            ? SpawnPattern.Circle
            : SpawnPattern.ZigZag;

        formationIndex++;

        Vector2 direction = Random.insideUnitCircle.normalized;

        if (direction.sqrMagnitude < 0.01f)
            direction = Vector2.right;

        Vector3 center = player.position + (Vector3)(direction * spawnDistance);
        int count = Mathf.Max(1, formationEnemyCount);
        int spawned = 0;

        for (int i = 0; i < count; i++)
        {
            if (!HasEnemyCapacity())
                break;

            Vector3 position;

            if (pattern == SpawnPattern.Circle)
            {
                float angle = i * Mathf.PI * 2f / count;

                position = center + new Vector3(
                    Mathf.Cos(angle) * circleRadius,
                    Mathf.Sin(angle) * circleRadius,
                    0f
                );
            }
            else
            {
                float x = (i % 2 == 0 ? -1f : 1f) * zigZagWidth * 0.5f;
                float y = (i / 2) * zigZagSpacing;

                position = center + new Vector3(x, y, 0f);
            }

            SpawnEnemyAt(position, true);
            spawned++;
        }

        Debug.Log(
            "Wave " + waveManager.CurrentWave + ": " +
            pattern + " formation, spawned " + spawned + " enemies."
        );
    }

    private bool HasEnemyCapacity()
    {
        int maxEnemies = Mathf.RoundToInt(
            baseMaxEnemies * waveManager.GetDifficultyMultiplier()
        );

        return GameObject.FindGameObjectsWithTag("Enemy").Length < maxEnemies;
    }

    private void SpawnEnemyAt(Vector3 position, bool useFormationDelay)
    {
        GameObject prefab = GetRandomEnemy();

        if (prefab == null)
            return;

        GameObject enemy = Instantiate(prefab, position, Quaternion.identity);
        EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();

        if (enemyBase != null)
        {
            enemyBase.moveSpeed *= waveManager.GetDifficultyMultiplier();

            if (useFormationDelay)
                enemyBase.HoldMovement(formationMovementDelay);
        }
    }

    private GameObject GetRandomEnemy()
    {
        WaveData currentWave = waveManager.CurrentWaveData;

        if (currentWave == null ||
            currentWave.enemies == null ||
            currentWave.enemies.Length == 0)
            return null;

        float totalChance = 0f;

        foreach (EnemySpawnData enemy in currentWave.enemies)
        {
            if (enemy.enemyPrefab != null)
                totalChance += Mathf.Max(0f, enemy.spawnChance);
        }

        if (totalChance <= 0f)
            return null;

        float randomValue = Random.Range(0f, totalChance);
        float currentChance = 0f;

        foreach (EnemySpawnData enemy in currentWave.enemies)
        {
            if (enemy.enemyPrefab == null)
                continue;

            currentChance += Mathf.Max(0f, enemy.spawnChance);

            if (randomValue <= currentChance)
                return enemy.enemyPrefab;
        }

        return null;
    }
}
