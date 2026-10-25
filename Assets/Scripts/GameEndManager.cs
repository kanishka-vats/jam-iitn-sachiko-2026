
using System.Collections;
using UnityEngine;

public class GameEndManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private DarknessProgressionManager darknessManager;
    [SerializeField] private EndingSequenceManager endingSequenceManager;

    [Header("End Screens")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject victoryScreen;

    [Header("Gameplay UI")]
    [SerializeField] private GameObject[] gameplayUI;

    [Header("Transition")]
    [SerializeField] private float blackoutDuration = 2.5f;

    private bool endingStarted;

    private void Start()
    {
        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);

        if (victoryScreen != null)
            victoryScreen.SetActive(false);

        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();

        if (darknessManager == null)
            darknessManager = FindFirstObjectByType<DarknessProgressionManager>();

        if (endingSequenceManager == null)
            endingSequenceManager = FindFirstObjectByType<EndingSequenceManager>();

        if (playerHealth != null)
            playerHealth.OnDeath += ShowGameOver;
        else
            Debug.LogError("GameEndManager: PlayerHealth reference is missing.");

        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnDeath -= ShowGameOver;
    }

    public void ShowGameOver()
    {
        if (endingStarted)
            return;

        endingStarted = true;
        Time.timeScale = 0f;

        if (playerController != null)
            playerController.SetMovementEnabled(false);

        HideGameplayUI();

        if (victoryScreen != null)
            victoryScreen.SetActive(false);

        if (darknessManager != null)
        {
            darknessManager.CollapseToBlack(blackoutDuration);
            StartCoroutine(ShowGameOverAfterBlackout());
        }
        else
        {
            ShowGameOverScreen();
        }
    }

    private IEnumerator ShowGameOverAfterBlackout()
    {
        yield return new WaitForSecondsRealtime(blackoutDuration);
        darknessManager.SetEffectEnabled(false);
        ShowGameOverScreen();
    }

    private void ShowGameOverScreen()
    {
        if (gameOverScreen != null)
            gameOverScreen.SetActive(true);
    }

    public void ShowVictory()
    {
        if (endingStarted)
            return;

        endingStarted = true;
        Time.timeScale = 0f;

        if (playerController != null)
            playerController.SetMovementEnabled(false);

        HideGameplayUI();

        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);

        if (victoryScreen != null)
            victoryScreen.SetActive(false);

        if (endingSequenceManager != null)
        {
            endingSequenceManager.PlayVictoryEnding();
        }
        else
        {
            Debug.LogError("GameEndManager: EndingSequenceManager is missing.");

            if (victoryScreen != null)
                victoryScreen.SetActive(true);
        }
    }

    private void HideGameplayUI()
    {
        if (gameplayUI == null)
            return;

        foreach (GameObject ui in gameplayUI)
        {
            if (ui != null)
                ui.SetActive(false);
        }
    }
}
