using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance;

    [Header("Power Ups")]
    [SerializeField] private List<PowerUpData> powerUps;

    [Header("Power Up Selection UI")]
    [SerializeField] private GameObject powerUpPanel;
    [SerializeField] private PowerUpCard[] powerUpCards;

    [Header("Reveal UI")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text resultText;

    [Header("Twisted Reveal")]
    [SerializeField] private GameObject twistedRevealObject;
    [SerializeField] private UIFrameAnimation twistedCharacterAnimation;
    [SerializeField] private TMP_Text twistedSubtitle;

    [Header("Twisted Effect Text")]
    [SerializeField] private TMP_Text twistedEffectText;

    [Header("Twisted Reveal Volume")]
    [SerializeField] private Volume twistedRevealVolume;

    [Header("Twisted Dialogue")]
    [SerializeField] private AudioSource twistedDialogueSource;
    [SerializeField] private AudioClip twistedDialogueClip;

    [Header("Twisted Reveal Settings")]
    [Range(0.05f, 1f)]
    [SerializeField] private float twistedTimeScale = 0.2f;

    [Header("References")]
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Gun playerGun;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Health Regeneration")]
    [SerializeField] private float regenerationInterval = 1f;

    private PowerUpData selectedPowerUp;

    private Coroutine healthRegenerationCoroutine;

    private float originalTimeScale;
    private float originalFixedDeltaTime;

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

        if (twistedRevealObject != null)
        {
            twistedRevealObject.SetActive(false);
        }

        if (twistedSubtitle != null)
        {
            twistedSubtitle.gameObject.SetActive(false);
        }

        if (twistedEffectText != null)
        {
            twistedEffectText.gameObject.SetActive(false);
        }

        if (twistedRevealVolume != null)
        {
            twistedRevealVolume.weight = 0f;
        }

        if (twistedDialogueSource != null)
        {
            twistedDialogueSource.playOnAwake = false;
            twistedDialogueSource.loop = false;
        }
    }

    public void GetRandomPowerUps()
    {
        if (powerUps == null || powerUps.Count < 3)
        {
            Debug.LogWarning(
                "PowerUpManager: Not enough power-ups configured."
            );

            return;
        }

        List<PowerUpData> availablePowerUps =
            new List<PowerUpData>(powerUps);

        List<PowerUpData> selectedPowerUps =
            new List<PowerUpData>();

        List<PowerUpData> twistedPowerUps =
            availablePowerUps.FindAll(
                powerUp => powerUp.isTwisted
            );

        if (twistedPowerUps.Count > 0)
        {
            PowerUpData twistedPowerUp =
                twistedPowerUps[
                    Random.Range(0, twistedPowerUps.Count)
                ];

            selectedPowerUps.Add(twistedPowerUp);
            availablePowerUps.Remove(twistedPowerUp);
        }

        while (
            selectedPowerUps.Count < 3 &&
            availablePowerUps.Count > 0
        )
        {
            int randomIndex =
                Random.Range(
                    0,
                    availablePowerUps.Count
                );

            PowerUpData randomPowerUp =
                availablePowerUps[randomIndex];

            selectedPowerUps.Add(randomPowerUp);
            availablePowerUps.RemoveAt(randomIndex);
        }

        for (int i = 0; i < powerUpCards.Length; i++)
        {
            if (i < selectedPowerUps.Count)
            {
                powerUpCards[i].Setup(
                    selectedPowerUps[i],
                    this
                );
            }
        }
    }

    public void TryPurchasePowerUp(PowerUpData powerUp)
    {
        if (powerUp == null)
        {
            return;
        }

        SelectPowerUp(powerUp);
    }

    public void SelectPowerUp(PowerUpData powerUp)
    {
        if (powerUp == null)
        {
            return;
        }

        selectedPowerUp = powerUp;

        if (powerUpPanel != null)
        {
            powerUpPanel.SetActive(false);
        }

        if (playerController != null)
        {
            playerController.SetMovementEnabled(true);
        }

        if (waveManager != null)
        {
            waveManager.StartNextWave();
        }

        StartCoroutine(RevealPowerUp());
    }

    private IEnumerator RevealPowerUp()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }

        float countdown = 10f;

        while (countdown > 0f)
        {
            if (countdownText != null)
            {
                countdownText.text =
                    Mathf.CeilToInt(countdown).ToString();
            }

            countdown -= Time.deltaTime;

            yield return null;
        }

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        RevealSelectedPowerUp();
    }

    private void RevealSelectedPowerUp()
    {
        if (selectedPowerUp == null)
        {
            return;
        }

        if (selectedPowerUp.isTwisted)
        {
            StartCoroutine(PlayTwistedReveal());
        }
        else
        {
            ShowNormalReveal();
            ApplyPowerUp();
        }
    }

    private void ShowNormalReveal()
    {
        if (resultText == null)
        {
            return;
        }

        resultText.gameObject.SetActive(true);
        resultText.text = "POWER-UP GRANTED!";
    }

    private IEnumerator PlayTwistedReveal()
    {
        originalTimeScale = Time.timeScale;
        originalFixedDeltaTime = Time.fixedDeltaTime;

        if (resultText != null)
        {
            resultText.gameObject.SetActive(true);
            resultText.text = "PLOT TWIST!";

            StartCoroutine(
                AnimatePlotTwistText()
            );
        }

        if (twistedRevealObject != null)
        {
            twistedRevealObject.SetActive(true);
        }

        if (twistedSubtitle != null)
        {
            twistedSubtitle.text =
                "YOU THOUGHT YOU GOT A POWER\nNO NO HOW IS THAT POSSIBLE WHEN I AM HERE";

            twistedSubtitle.gameObject.SetActive(true);
        }

        if (twistedEffectText != null)
        {
            twistedEffectText.text =
                GetTwistedEffectText();

            twistedEffectText.gameObject.SetActive(true);
        }

        if (twistedRevealVolume != null)
        {
            twistedRevealVolume.weight = 1f;

            Debug.Log(
                "PowerUpManager: RED TINT ENABLED."
            );
        }
        else
        {
            Debug.LogWarning(
                "PowerUpManager: Twisted Reveal Volume is not assigned."
            );
        }

        Time.timeScale = twistedTimeScale;

        Time.fixedDeltaTime =
            originalFixedDeltaTime *
            twistedTimeScale;

        if (twistedCharacterAnimation != null)
        {
            twistedCharacterAnimation.Play();
        }
        else
        {
            Debug.LogWarning(
                "PowerUpManager: Twisted Character Animation is not assigned."
            );
        }

        if (
            twistedDialogueSource != null &&
            twistedDialogueClip != null
        )
        {
            twistedDialogueSource.clip =
                twistedDialogueClip;

            twistedDialogueSource.Play();
        }
        else
        {
            Debug.LogWarning(
                "PowerUpManager: Twisted dialogue source or clip is missing."
            );
        }

        if (
            twistedDialogueSource != null &&
            twistedDialogueSource.clip != null
        )
        {
            yield return new WaitWhile(
                () => twistedDialogueSource.isPlaying
            );
        }
        else
        {
            yield return new WaitForSecondsRealtime(2f);
        }

        if (twistedCharacterAnimation != null)
        {
            twistedCharacterAnimation.Stop();
        }

        if (twistedRevealObject != null)
        {
            twistedRevealObject.SetActive(false);
        }

        if (twistedSubtitle != null)
        {
            twistedSubtitle.gameObject.SetActive(false);
        }

        if (twistedEffectText != null)
        {
            twistedEffectText.gameObject.SetActive(false);
        }

        if (twistedRevealVolume != null)
        {
            twistedRevealVolume.weight = 0f;

            Debug.Log(
                "PowerUpManager: RED TINT DISABLED."
            );
        }

        Time.timeScale = originalTimeScale;

        Time.fixedDeltaTime =
            originalFixedDeltaTime;

        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }

        ApplyPowerUp();
    }

    private IEnumerator AnimatePlotTwistText()
    {
        if (resultText == null)
        {
            yield break;
        }

        Vector3 originalScale =
            resultText.transform.localScale;

        resultText.transform.localScale =
            originalScale * 0.5f;

        float duration = 0.35f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / duration
                );

            float eased =
                1f -
                Mathf.Pow(
                    1f - progress,
                    3f
                );

            resultText.transform.localScale =
                Vector3.Lerp(
                    originalScale * 0.5f,
                    originalScale,
                    eased
                );

            yield return null;
        }

        resultText.transform.localScale =
            originalScale;
    }

    private string GetTwistedEffectText()
    {
        if (selectedPowerUp == null)
        {
            return "";
        }

        switch (selectedPowerUp.powerUpType)
        {
            case PowerUpType.Invisibility:
                return "YOU REALLY THOUGHT THAT WOULD WORK?";

            case PowerUpType.InvertedControls:
                return "INVERTED CONTROLS!";

            case PowerUpType.Heal:
                return "YOU REALLY THOUGHT THAT WOULD HELP?";

            default:
                return "YOU HAVE NO IDEA WHAT YOU JUST CHOSE.";
        }
    }

    private void ApplyPowerUp()
    {
        if (selectedPowerUp == null)
        {
            return;
        }

        switch (selectedPowerUp.powerUpType)
        {
            case PowerUpType.Damage:

                if (playerGun != null)
                {
                    playerGun.AddDamageMultiplier(
                        selectedPowerUp.effectValue
                    );
                }

                break;

            case PowerUpType.ProjectileSpeed:

                if (playerGun != null)
                {
                    playerGun.AddBulletSpeedMultiplier(
                        selectedPowerUp.effectValue
                    );
                }

                break;

            case PowerUpType.ProjectileSize:

                if (playerGun != null)
                {
                    playerGun.AddBulletSizeMultiplier(
                        selectedPowerUp.effectValue
                    );
                }

                break;

            case PowerUpType.Heal:

                StartHealthRegeneration();

                if (waveManager != null)
                {
                    waveManager.AddWaveTime(25f);
                }

                break;

            case PowerUpType.Invisibility:

                Debug.Log(
                    "Invisibility power-up activated."
                );

                break;

            case PowerUpType.InvertedControls:

                if (playerController != null)
                {
                    playerController.SetInvertedControls(
                        true
                    );
                }

                break;

            case PowerUpType.FuturePowerUp1:

                Debug.Log(
                    "FuturePowerUp1 activated."
                );

                break;

            case PowerUpType.FuturePowerUp2:

                Debug.Log(
                    "FuturePowerUp2 activated."
                );

                break;
        }
    }

    private void StartHealthRegeneration()
    {
        StopHealthRegeneration();

        healthRegenerationCoroutine =
            StartCoroutine(
                RegenerateHealth()
            );
    }

    private IEnumerator RegenerateHealth()
    {
        while (playerHealth != null)
        {
            yield return new WaitForSeconds(
                regenerationInterval
            );

            if (playerHealth != null)
            {
                playerHealth.Heal(1);
            }
        }
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

    public void ResetTemporaryPowerUps()
    {
        StopHealthRegeneration();

        if (playerController != null)
        {
            playerController.SetInvertedControls(false);
        }
    }

    public void ShowPowerUpSelection()
    {
        if (powerUpPanel != null)
        {
            powerUpPanel.SetActive(true);
        }

        if (playerController != null)
        {
            playerController.SetMovementEnabled(false);
        }

        GetRandomPowerUps();
    }
}