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

    // Runtime pool.
    // Selected power-ups are removed from this list.
    private List<PowerUpData> remainingPowerUps =
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
        remainingPowerUps =
            new List<PowerUpData>(powerUps);

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
            remainingPowerUps.FindAll(
                powerUp => !powerUp.isTwisted
            );

        List<PowerUpData> twistedPowerUps =
            remainingPowerUps.FindAll(
                powerUp => powerUp.isTwisted
            );

        if (realPowerUps.Count == 0 ||
            twistedPowerUps.Count == 0)
        {
            Debug.LogWarning(
                "Not enough real or twisted power-ups remaining."
            );

            return selectedPowerUps;
        }

        // Guarantee one twisted power-up.
        int twistedIndex =
            Random.Range(0, twistedPowerUps.Count);

        selectedPowerUps.Add(
            twistedPowerUps[twistedIndex]
        );

        List<PowerUpData> availableForSelection =
            new List<PowerUpData>(remainingPowerUps);

        availableForSelection.Remove(
            selectedPowerUps[0]
        );

        while (selectedPowerUps.Count < amount &&
               availableForSelection.Count > 0)
        {
            int randomIndex =
                Random.Range(
                    0,
                    availableForSelection.Count
                );

            selectedPowerUps.Add(
                availableForSelection[randomIndex]
            );

            availableForSelection.RemoveAt(randomIndex);
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


    // --------------------------------------------------
    // PLAYER SELECTS POWER-UP
    // --------------------------------------------------

    public void SelectPowerUp(PowerUpData powerUp)
    {
        if (powerUp == null)
            return;

        selectedPowerUp = powerUp;

        // Remove selected power-up from future selections.
        remainingPowerUps.Remove(powerUp);

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

                // Start slow health regeneration.
                if (playerHealth != null)
                {
                    healthRegenerationCoroutine =
                        StartCoroutine(
                            RegenerateHealth()
                        );
                }

                // Plot twist:
                // Increase the current wave timer by 25 seconds.
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
}