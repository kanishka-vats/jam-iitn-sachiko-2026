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
    [SerializeField] private Gun playerGun;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Regenerative Health")]
    [SerializeField] private float regenerationInterval = 1f;

    private PowerUpData selectedPowerUp;

    private Coroutine healthRegenerationCoroutine;


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

        if (powerUps == null || powerUps.Count < amount)
        {
            Debug.LogWarning(
                "Not enough power-ups available."
            );

            return selectedPowerUps;
        }

        int currentDiamonds = 0;

        if (CurrencyManager.Instance != null)
        {
            currentDiamonds =
                CurrencyManager.Instance.Diamonds;
        }

        // Separate twisted and normal power-ups
        List<PowerUpData> realPowerUps =
            powerUps.FindAll(
                powerUp => !powerUp.isTwisted
            );

        List<PowerUpData> twistedPowerUps =
            powerUps.FindAll(
                powerUp => powerUp.isTwisted
            );

        // Find power-ups the player can currently afford
        List<PowerUpData> affordablePowerUps =
            powerUps.FindAll(
                powerUp =>
                    powerUp.diamondCost <= currentDiamonds
            );

        // ------------------------------------------------
        // 1. GUARANTEE AT LEAST ONE AFFORDABLE CARD
        // ------------------------------------------------

        if (affordablePowerUps.Count > 0)
        {
            PowerUpData affordableCard =
                affordablePowerUps[
                    Random.Range(
                        0,
                        affordablePowerUps.Count
                    )
                ];

            selectedPowerUps.Add(
                affordableCard
            );
        }
        else
        {
            Debug.LogWarning(
                "Player cannot afford any power-up!"
            );

            // Fallback: give the player the cheapest card
            PowerUpData cheapestPowerUp =
                powerUps[0];

            foreach (PowerUpData powerUp in powerUps)
            {
                if (powerUp.diamondCost <
                    cheapestPowerUp.diamondCost)
                {
                    cheapestPowerUp = powerUp;
                }
            }

            selectedPowerUps.Add(
                cheapestPowerUp
            );
        }

        // ------------------------------------------------
        // 2. GUARANTEE AT LEAST ONE TWISTED CARD
        // ------------------------------------------------

        if (twistedPowerUps.Count > 0)
        {
            bool alreadyHasTwisted =
                selectedPowerUps.Exists(
                    powerUp => powerUp.isTwisted
                );

            if (!alreadyHasTwisted)
            {
                List<PowerUpData> availableTwisted =
                    new List<PowerUpData>(
                        twistedPowerUps
                    );

                availableTwisted.RemoveAll(
                    powerUp =>
                        selectedPowerUps.Contains(powerUp)
                );

                if (availableTwisted.Count > 0)
                {
                    PowerUpData twistedCard =
                        availableTwisted[
                            Random.Range(
                                0,
                                availableTwisted.Count
                            )
                        ];

                    selectedPowerUps.Add(
                        twistedCard
                    );
                }
            }
        }

        // ------------------------------------------------
        // 3. FILL REMAINING CARDS RANDOMLY
        // ------------------------------------------------

        List<PowerUpData> remainingPowerUps =
            new List<PowerUpData>(powerUps);

        remainingPowerUps.RemoveAll(
            powerUp =>
                selectedPowerUps.Contains(powerUp)
        );

        while (
            selectedPowerUps.Count < amount &&
            remainingPowerUps.Count > 0
        )
        {
            int randomIndex =
                Random.Range(
                    0,
                    remainingPowerUps.Count
                );

            selectedPowerUps.Add(
                remainingPowerUps[randomIndex]
            );

            remainingPowerUps.RemoveAt(randomIndex);
        }

        return selectedPowerUps;
    }


    // --------------------------------------------------
    // PURCHASE POWER-UP
    // --------------------------------------------------

    public void TryPurchasePowerUp(PowerUpData powerUp)
    {
        if (powerUp == null)
            return;

        // Make sure CurrencyManager exists.
        if (CurrencyManager.Instance == null)
        {
            Debug.LogWarning(
                "CurrencyManager is missing."
            );

            return;
        }

        // Check if player has enough diamonds.
        if (!CurrencyManager.Instance.CanAfford(
            powerUp.diamondCost))
        {
            Debug.Log(
                "NOT ENOUGH DIAMONDS! " +
                powerUp.powerUpName +
                " costs " +
                powerUp.diamondCost +
                " diamonds."
            );

            return;
        }

        // Spend diamonds.
        bool purchaseSuccessful =
            CurrencyManager.Instance.SpendDiamonds(
                powerUp.diamondCost
            );

        if (!purchaseSuccessful)
        {
            return;
        }

        Debug.Log(
            "POWER-UP PURCHASED: " +
            powerUp.powerUpName +
            " for " +
            powerUp.diamondCost +
            " diamonds."
        );

        // Continue with the existing selection flow.
        SelectPowerUp(powerUp);
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

        if (playerController != null)
        {
            playerController.SetMovementEnabled(true);
        }

        // Start the next wave immediately.
        if (waveManager != null)
        {
            waveManager.StartNextWave();
        }

        // Start the reveal at the same time.
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
                if (selectedPowerUp.powerUpType ==
                    PowerUpType.Invisibility)
                {
                    resultText.text =
                        "YOU REALLY THOUGHT THAT WOULD WORK?";
                }
                else
                {
                    resultText.text =
                        "PLOT TWIST!";
                }
            }
            else
            {
                resultText.text =
                    "POWER-UP GRANTED!";
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

        ApplyPowerUp();
    }


    // --------------------------------------------------
    // APPLY POWER-UP
    // --------------------------------------------------

    private void ApplyPowerUp()
    {
        if (selectedPowerUp == null)
            return;

        switch (selectedPowerUp.powerUpType)
        {
            // ------------------------------------------
            // LEGIT: DAMAGE
            // ------------------------------------------

            case PowerUpType.Damage:

                if (playerGun != null)
                {
                    playerGun.AddDamageMultiplier(
                        selectedPowerUp.effectValue
                    );
                }

                Debug.Log(
                    "Damage increased by " +
                    selectedPowerUp.effectValue
                );

                break;


            // ------------------------------------------
            // LEGIT: PROJECTILE SPEED
            // ------------------------------------------

            case PowerUpType.ProjectileSpeed:

                if (playerGun != null)
                {
                    playerGun.AddBulletSpeedMultiplier(
                        selectedPowerUp.effectValue
                    );
                }

                Debug.Log(
                    "Projectile speed increased by " +
                    selectedPowerUp.effectValue
                );

                break;


            // ------------------------------------------
            // LEGIT: PROJECTILE SIZE
            // ------------------------------------------

            case PowerUpType.ProjectileSize:

                if (playerGun != null)
                {
                    playerGun.AddBulletSizeMultiplier(
                        selectedPowerUp.effectValue
                    );
                }

                Debug.Log(
                    "Projectile size increased by " +
                    selectedPowerUp.effectValue
                );

                break;


            // ------------------------------------------
            // TWISTED: REGENERATIVE HEALTH
            // ------------------------------------------

            case PowerUpType.Heal:

                if (playerHealth != null)
                {
                    healthRegenerationCoroutine =
                        StartCoroutine(
                            RegenerateHealth()
                        );
                }

                if (waveManager != null)
                {
                    waveManager.AddWaveTime(25f);
                }

                Debug.Log(
                    "PLOT TWIST: Regenerative health activated."
                );

                Debug.Log(
                    "PLOT TWIST: Wave timer increased by 25 seconds."
                );

                break;


            // ------------------------------------------
            // TWISTED: INVISIBILITY
            // ------------------------------------------

            case PowerUpType.Invisibility:

                Debug.Log(
                    "PLOT TWIST: Invisibility was fake!"
                );

                break;


            // ------------------------------------------
            // TWISTED: INVERTED CONTROLS
            // ------------------------------------------

            case PowerUpType.InvertedControls:

                if (playerController != null)
                {
                    playerController.SetInvertedControls(true);
                }

                Debug.Log(
                    "PLOT TWIST: Controls will be inverted " +
                    "for this wave."
                );

                break;
        }
    }


    // --------------------------------------------------
    // REGENERATIVE HEALTH
    // --------------------------------------------------

    private IEnumerator RegenerateHealth()
    {
        while (true)
        {
            yield return new WaitForSeconds(
                regenerationInterval
            );

            if (playerHealth == null)
                yield break;

            if (playerHealth.IsDead())
                yield break;

            playerHealth.Heal(1);

            Debug.Log(
                "Regenerative Health: +1 HP"
            );
        }
    }


    // --------------------------------------------------
    // RESET TEMPORARY POWER-UPS
    // --------------------------------------------------

    public void ResetTemporaryPowerUps()
    {
        Debug.Log(
            "===== RESETTING TEMPORARY POWER-UPS ====="
        );

        // Reset inverted controls.
        if (playerController != null)
        {
            playerController.SetInvertedControls(false);
        }

        // Stop regenerative health.
        StopHealthRegeneration();
    }


    private void StopHealthRegeneration()
    {
        if (healthRegenerationCoroutine != null)
        {
            StopCoroutine(
                healthRegenerationCoroutine
            );

            healthRegenerationCoroutine = null;
        }
    }

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

    powerUpPanel.SetActive(true);

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

    Debug.Log(
        "===== CHOOSE YOUR POWER-UP ====="
    );
}


}