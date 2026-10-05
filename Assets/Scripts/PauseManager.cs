using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("Pause")]
    [SerializeField] private GameObject pauseMenu;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject[] gameplayUI;

    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;

    private bool isPaused = false;

    private void Start()
    {
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        playerHealth.OnDeath += ShowGameOver;

        pauseMenu.SetActive(false);
        gameOverPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath -= ShowGameOver;
        }
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;

        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;

        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ShowGameOver()
    {
        Debug.Log("SHOW GAME OVER CALLED");

        isPaused = true;

        // Hide gameplay UI
        foreach (GameObject ui in gameplayUI)
        {
            if (ui != null)
            {
                ui.SetActive(false);
            }
        }

        // Hide pause menu
        pauseMenu.SetActive(false);

        // Show game over
        gameOverPanel.SetActive(true);

        // Freeze game
        Time.timeScale = 0f;
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}