using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private WaveManager waveManager;

    void Start()
    {
        
        waveManager = FindFirstObjectByType<WaveManager>();
    }

    
    public void DefeatEnemy()
    {
        
        if (waveManager != null)
        {
            waveManager.EnemyDefeated();
        }

       
        gameObject.SetActive(false);
    }
}