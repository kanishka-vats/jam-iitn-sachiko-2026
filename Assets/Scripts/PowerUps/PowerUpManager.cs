using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance;

    [Header("Available Power Ups")]
    [SerializeField] private List<PowerUpData> powerUps =
        new List<PowerUpData>();

    [Header("UI")]
    [SerializeField] private GameObject powerUpPanel;
    [SerializeField] private PowerUpCard[] powerUpCards;

    [Header("Reveal UI")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text resultText;

    [Header("References")]
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private PlayerController playerController;

    private PowerUpData selectedPowerUp;

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

    private void Start()
    {
        if (powerUpPanel != null)
        {
            powerUpPanel.SetActive(false);
        }

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }
    }

    // --------------------------------------------------
    // POWER-UP GENERATION
    // --------------------------------------------------

    public List<PowerUpData> GetRandomPowerUps(int amount)
    {
        List<PowerUpData> selectedPowerUps =
            new List<PowerUpData>();

        List<PowerUpData> realPowerUps =
            powerUps.FindAll(powerUp => !powerUp.isTwisted);

        List<PowerUpData> twistedPowerUps =
            powerUps.FindAll(powerUp => powerUp.isTwisted);

        if (realPowerUps.Count == 0 ||
            twistedPowerUps.Count == 0)
        {
            Debug.LogWarning(
                "Not enough real or twisted power-ups available."
            );

            return selectedPowerUps;
        }

        // Guarantee one twisted power-up.
        int twistedIndex =
            Random.Range(0, twistedPowerUps.Count);

        selectedPowerUps.Add(
            twistedPowerUps[twistedIndex]
        );

        List<PowerUpData> remainingPowerUps =
            new List<PowerUpData>(powerUps);

        remainingPowerUps.Remove(
            selectedPowerUps[0]
        );

        while (selectedPowerUps.Count < amount &&
               remainingPowerUps.Count > 0)
        {
            int randomIndex =
                Random.Range(0, remainingPowerUps.Count);

            selectedPowerUps.Add(
                remainingPowerUps[randomIndex]
            );

            remainingPowerUps.RemoveAt(randomIndex);
        }

        return selectedPowerUps;
    }

    // --------------------------------------------------
    // SHOW POWER-UP SELECTION
    // --------------------------------------------------

    public void ShowPowerUpSelection()
    {
        if (powerUpPanel == null)
        {
            Debug.LogWarning(
                "Power Up Panel is not assigned."
            );

            return;
        }

        List<PowerUpData> selectedPowerUps =
            GetRandomPowerUps(3);

        if (selectedPowerUps.Count < 3)
        {
            Debug.LogWarning(
                "Could not generate 3 power-ups."
            );

            return;
        }

        // Show power-up selection UI.
        powerUpPanel.SetActive(true);

        // Disable player movement while choosing.
        if (playerController != null)
        {
            playerController.SetMovementEnabled(false);
        }

        for (int i = 0; i < powerUpCards.Length; i++)
        {
            if (i < selectedPowerUps.Count)
            {
                powerUpCards[i].gameObject.SetActive(true);

                powerUpCards[i].Setup(
                    selectedPowerUps[i],
                    this
                );
            }
            else
            {
                powerUpCards[i].gameObject.SetActive(false);
            }
        }

        Debug.Log("===== CHOOSE YOUR POWER-UP =====");
    }

    // --------------------------------------------------
    // PLAYER SELECTS POWER-UP
    // --------------------------------------------------

    public void SelectPowerUp(PowerUpData powerUp)
    {
        if (powerUp == null)
            return;

        selectedPowerUp = powerUp;

        Debug.Log(
            "POWER-UP SELECTED: " +
            selectedPowerUp.powerUpName
        );

        if (powerUpPanel != null)
        {
            powerUpPanel.SetActive(false);
        }

        // Re-enable player movement immediately.
        if (playerController != null)
        {
            playerController.SetMovementEnabled(true);
        }

        // Start the next wave immediately.
        if (waveManager != null)
        {
            waveManager.StartNextWave();
        }

        // Start the 10-second reveal at the same time.
        StartCoroutine(RevealPowerUp());
    }

    // --------------------------------------------------
    // POWER-UP REVEAL
    // --------------------------------------------------

    private IEnumerator RevealPowerUp()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }

        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }

        float timer = 10f;

        while (timer > 0f)
        {
            if (countdownText != null)
            {
                countdownText.text =
                    "POWER-UP REVEAL\n" +
                    Mathf.CeilToInt(timer);
            }

            timer -= Time.deltaTime;

            yield return null;
        }

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        RevealSelectedPowerUp();
    }

    // --------------------------------------------------
    // REVEAL RESULT
    // --------------------------------------------------

    private void RevealSelectedPowerUp()
    {
        if (selectedPowerUp == null)
            return;

        if (resultText != null)
        {
            resultText.gameObject.SetActive(true);

            if (selectedPowerUp.isTwisted)
            {
                resultText.text = "PLOT TWIST!";
            }
            else
            {
                resultText.text = "POWER-UP GRANTED!";
            }
        }

        Debug.Log(
            "POWER-UP REVEALED: " +
            selectedPowerUp.powerUpName
        );

        if (selectedPowerUp.isTwisted)
        {
            Debug.Log("PLOT TWIST!");
        }
        else
        {
            Debug.Log("POWER-UP GRANTED!");
        }

        // Actual power-up effects will be added here later.
    }
}