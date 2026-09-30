using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance;

    [Header("Available Power Ups")]
    public List<PowerUpData> powerUps = new List<PowerUpData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public List<PowerUpData> GetRandomPowerUps(int amount)
    {
        List<PowerUpData> selectedPowerUps = new List<PowerUpData>();

        List<PowerUpData> realPowerUps = powerUps.FindAll(powerUp => !powerUp.isTwisted);
        List<PowerUpData> twistedPowerUps = powerUps.FindAll(powerUp => powerUp.isTwisted);

        // Make sure we have at least one of each type
        if (realPowerUps.Count == 0 || twistedPowerUps.Count == 0)
        {
            Debug.LogWarning("Not enough real or twisted power-ups available.");
            return selectedPowerUps;
        }

        // Add one guaranteed twisted power-up
        int twistedIndex = Random.Range(0, twistedPowerUps.Count);
        selectedPowerUps.Add(twistedPowerUps[twistedIndex]);

        // Add the remaining power-ups randomly
        List<PowerUpData> remainingPowerUps = new List<PowerUpData>(powerUps);

        remainingPowerUps.Remove(selectedPowerUps[0]);

        while (selectedPowerUps.Count < amount && remainingPowerUps.Count > 0)
        {
            int randomIndex = Random.Range(0, remainingPowerUps.Count);

            selectedPowerUps.Add(remainingPowerUps[randomIndex]);
            remainingPowerUps.RemoveAt(randomIndex);
        }

        return selectedPowerUps;
    }

    [ContextMenu("Test Random Power Ups")]
    private void TestRandomPowerUps()
    {
        List<PowerUpData> selectedPowerUps = GetRandomPowerUps(3);

        foreach (PowerUpData powerUp in selectedPowerUps)
        {
            Debug.Log("Selected Power Up: " + powerUp.powerUpName);
        }
    }
}