
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("Pause")]
    [SerializeField] private GameObject pauseMenu;

    private bool isPaused;
    private bool pauseAllowed = true;

    private void Start()
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame &&
            pauseAllowed)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (!pauseAllowed || isPaused)
            return;

        isPaused = true;

        if (pauseMenu != null)
            pauseMenu.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        Time.timeScale = 1f;
    }

    public void DisablePause()
    {
        pauseAllowed = false;
        isPaused = false;

        if (pauseMenu != null)
            pauseMenu.SetActive(false);
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}
