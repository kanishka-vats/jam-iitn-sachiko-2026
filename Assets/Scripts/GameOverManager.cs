using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameObject gameOverScreen;

    private void Start()
    {
        gameOverScreen.SetActive(false);

        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

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

    private void ShowGameOver()
    {
        gameOverScreen.SetActive(true);

        Time.timeScale = 0f;
    }
}