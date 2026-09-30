using UnityEngine;

[CreateAssetMenu(fileName = "NewWave", menuName = "Waves/Wave Data")]
public class WaveData : ScriptableObject
{
    [Header("Wave Settings")]
    public string waveName;
    public float duration = 30f;

    [Header("Difficulty")]
    public float difficultyMultiplier = 1f;

    [Header("Enemies")]
    public EnemySpawnData[] enemies;

    [Header("Boss")]
    public bool isBossWave = false;
}