using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Configuration")]
    [SerializeField] private WaveData[] waves;

    [Header("Final Wave")]
    [SerializeField] private int finalWave = 4;

    [Header("References")]
    [SerializeField] private PowerUpManager powerUpManager;
    [SerializeField] private GameEndManager gameEndManager;

    [Header("Current Wave")]
    [SerializeField] private int currentWave = 1;

    [Header("Difficulty")]
    [SerializeField] private float difficultyPerWave = 0.15f;

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
                "No WaveData found for wave " +
                currentWave
            );

            return;
        }

        waveTimer = wave.duration;
        waveActive = true;

        Debug.Log(
            "===== WAVE " +
            currentWave +
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
                "===== WAVE " +
                currentWave +
                " COMPLETE ====="
            );
        }

        // ------------------------------------------
        // FINAL WAVE
        // ------------------------------------------

        if (currentWave >= finalWave)
        {
            Debug.Log(
                "===== ALL WAVES COMPLETE ====="
            );

            if (gameEndManager != null)
            {
                gameEndManager.ShowVictory();
            }
            else
            {
                Debug.LogWarning(
                    "GameEndManager reference is missing."
                );
            }

            return;
        }

        // ------------------------------------------
        // NORMAL WAVE
        // ------------------------------------------

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
        Debug.Log(
            "START NEXT WAVE CALLED. Current wave BEFORE increment: " +
            currentWave
        );

        currentWave++;

        Debug.Log(
            "Current wave AFTER increment: " +
            currentWave
        );

        // ------------------------------------------
        // DO NOT START WAVE 5
        // ------------------------------------------

        if (currentWave > finalWave)
        {
            Debug.Log(
                "===== FINAL WAVE ALREADY COMPLETE ====="
            );

            if (gameEndManager != null)
            {
                gameEndManager.ShowVictory();
            }

            return;
        }

        // Reset temporary power-ups
        if (powerUpManager != null)
        {
            powerUpManager.ResetTemporaryPowerUps();
        }

        StartWave();
    }

    // --------------------------------------------------
    // WAVE TIMER MODIFICATION
    // --------------------------------------------------

    public void AddWaveTime(float amount)
    {
        if (!waveActive)
        {
            Debug.LogWarning(
                "Cannot add wave time because the wave is not active."
            );

            return;
        }

        waveTimer += amount;

        Debug.Log(
            "Wave timer increased by " +
            amount +
            " seconds. " +
            "New time: " +
            waveTimer
        );
    }

    public float GetDifficultyMultiplier()
    {
        return 1f +
            (currentWave - 1) *
            difficultyPerWave;
    }
}