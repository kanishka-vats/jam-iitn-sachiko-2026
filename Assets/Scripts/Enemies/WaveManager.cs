using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public int currentWave = 1;
    public float waveDuration = 60f;

    [Header("Difficulty")]
    public float difficultyIncreasePerWave = 0.25f;

    private float waveTimer;
    private bool waveActive = true;

    public float WaveTimer => waveTimer;
    public bool WaveActive => waveActive;

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

    private void StartWave()
    {
        waveTimer = waveDuration;
        waveActive = true;

        Debug.Log("===== WAVE " + currentWave + " STARTED =====");
    }

    private void EndWave()
    {
        waveActive = false;

        Debug.Log("===== WAVE " + currentWave + " COMPLETE =====");

        // Power-up selection will be triggered here later.
    }

    public float GetDifficultyMultiplier()
    {
        return 1f + ((currentWave - 1) * difficultyIncreasePerWave);
    }

    public void StartNextWave()
    {
        currentWave++;

        StartWave();
    }
}