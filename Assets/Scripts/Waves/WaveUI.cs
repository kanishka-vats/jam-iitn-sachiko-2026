using TMPro;
using UnityEngine;

public class WaveUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text waveTimerText;

    private void Update()
    {
        if (waveManager == null)
            return;

        UpdateWaveText();
        UpdateTimerText();
    }

    private void UpdateWaveText()
    {
        WaveData currentWave = waveManager.CurrentWaveData;

        if (currentWave == null)
            return;

        waveText.text = currentWave.waveName;
    }

    private void UpdateTimerText()
    {
        float timeRemaining = Mathf.Max(
            0f,
            waveManager.WaveTimer
        );

        int seconds = Mathf.CeilToInt(timeRemaining);

        waveTimerText.text = seconds.ToString();
    }
}