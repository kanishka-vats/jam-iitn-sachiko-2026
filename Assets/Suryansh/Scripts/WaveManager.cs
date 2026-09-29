using UnityEngine;
using UnityEngine.Events;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public int currentWave = 1;
    public int baseEnemiesPerWave = 5;
    public float timeBetweenWaves = 5f;

    [Header("Events (Team can hook UI into these!)")]
    public UnityEvent<int> OnWaveStarted;
    public UnityEvent OnWaveCompleted;
    
    private int enemiesRemainingAlive;

    void Start()
    {
        StartNextWave();
    }

    public void StartNextWave()
    {
        
        int enemiesToSpawn = baseEnemiesPerWave + (currentWave * 2); 
        enemiesRemainingAlive = enemiesToSpawn;

        Debug.Log($"Wave {currentWave} Starting with {enemiesToSpawn} enemies!");
        
       
        OnWaveStarted.Invoke(enemiesToSpawn);
    }

   
    public void EnemyDefeated()
    {
        enemiesRemainingAlive--;
        if (enemiesRemainingAlive <= 0)
        {
            WaveComplete();
        }
    }

    private void WaveComplete()
    {
        Debug.Log("Wave Cleared! Waiting for next wave...");
        OnWaveCompleted.Invoke();
        currentWave++;
        Invoke(nameof(StartNextWave), timeBetweenWaves); // Wave transition delay
    }
}