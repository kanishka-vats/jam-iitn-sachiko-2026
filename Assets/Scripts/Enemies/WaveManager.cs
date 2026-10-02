using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Configuration")]
    [SerializeField] private WaveData[] waves;

    [Header("References")]
    [SerializeField] private PowerUpManager powerUpManager; 

    [Header("Current Wave")]
    [SerializeField] private int currentWave = 1;

    private float waveTimer;
    private bool waveActive = false;

    public float WaveTimer => waveTimer;
    public bool WaveActive => waveActive;

    public int CurrentWave => currentWave;

    public WaveData CurrentWaveData
    {
        get
        {
            if (waves == null || waves.Length == 0)
                return null;

            int index = currentWave - 1;

            if (index < 0 || index >= waves.Length)
                return null;

            return waves[index];
        }
    }

    private void Start()
    {
        StartWave();
    }

    private void Update()
    {
        if (!waveActive)
            return;

        waveTimer -= Time.deltaTime;

        if (waveTimer <= 0f)
        {
            waveTimer = 0f;
            EndWave();
        }
    }

    public void StartWave()
    {
        WaveData wave = CurrentWaveData;

        if (wave == null)
        {
            Debug.LogWarning(
                "No WaveData found for wave " + currentWave
            );

            return;
        }

        waveTimer = wave.duration;
        waveActive = true;

        Debug.Log(
            "===== " +
            wave.waveName +
            " STARTED ====="
        );
    }

    private void EndWave()
    {
        waveActive = false;

        DestroyRemainingEnemies();

        WaveData wave = CurrentWaveData;

        if (wave != null)
        {
            Debug.Log(
                "===== " +
                wave.waveName +
                " COMPLETE ====="
            );
        }

        if (powerUpManager != null)
        {
            powerUpManager.ShowPowerUpSelection();
        }
    }

    private void DestroyRemainingEnemies()
    {
        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }

        Debug.Log(
            "Remaining enemies cleared: " +
            enemies.Length
        );
    }


    public void StartNextWave()
    {
        currentWave++;

        if (currentWave > waves.Length)
        {
            Debug.Log("===== ALL WAVES COMPLETE =====");
            return;
        }

        StartWave();
    }

    public float GetDifficultyMultiplier()
    {
        WaveData wave = CurrentWaveData;

        if (wave == null)
            return 1f;

        return wave.difficultyMultiplier;
    }

    public bool IsBossWave()
    {
        WaveData wave = CurrentWaveData;

        if (wave == null)
            return false;

        return wave.isBossWave;
    }
}