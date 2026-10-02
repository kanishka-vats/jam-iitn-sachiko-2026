using UnityEngine;

public class GameEndManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("End Screens")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject victoryScreen;

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
        gameOverScreen.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ShowVictory()
    {
        victoryScreen.SetActive(true);

        Time.timeScale = 0f;
    }
}