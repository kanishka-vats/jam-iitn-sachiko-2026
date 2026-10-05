using UnityEngine;

public class GameEndManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("End Screens")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject victoryScreen;

    [Header("Gameplay UI")]
    [SerializeField] private GameObject[] gameplayUI;

    private void Start()
    {
        // Hide both screens at the start.
        gameOverScreen.SetActive(false);
        victoryScreen.SetActive(false);

        // Find PlayerHealth automatically if not assigned.
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        // Listen for player death.
        if (playerHealth != null)
        {
            playerHealth.OnDeath += ShowGameOver;
        }

        // Make sure the game starts unpaused.
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath -= ShowGameOver;
        }
    }

    public void ShowGameOver()
    {
        // Hide all gameplay UI.
        HideGameplayUI();

        // Show Game Over screen.
        gameOverScreen.SetActive(true);

        // Pause game.
        Time.timeScale = 0f;
    }

    public void ShowVictory()
    {
        // Hide all gameplay UI.
        HideGameplayUI();

        // Show Victory screen.
        victoryScreen.SetActive(true);

        // Pause game.
        Time.timeScale = 0f;
    }

    private void HideGameplayUI()
    {
        foreach (GameObject ui in gameplayUI)
        {
            if (ui != null)
            {
                ui.SetActive(false);
            }
        }
    }
}