
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class StorySequenceManager : MonoBehaviour
{
    [Header("Story UI")]
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private GameObject[] storyPages;
    [SerializeField] private GameObject continueHint;

    [Header("Gameplay")]
    [SerializeField] private string gameSceneName = "Game";

    private int currentPage;
    private bool storyStarted;
    private float previousTimeScale;

    private void Awake()
    {
        if (storyPanel != null)
            storyPanel.SetActive(false);

        if (storyPages == null)
            return;

        foreach (GameObject page in storyPages)
        {
            if (page != null)
                page.SetActive(false);
        }
    }

    private void Update()
    {
        if (!storyStarted || Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
            ShowNextPage();
    }

    public void BeginStory()
    {
        if (storyStarted)
            return;

        if (storyPanel == null || storyPages == null ||
            storyPages.Length == 0)
        {
            Debug.LogError("Story UI or story pages are not assigned.", this);
            return;
        }

        foreach (GameObject page in storyPages)
        {
            if (page == null)
            {
                Debug.LogError("One or more story pages are missing.", this);
                return;
            }
        }

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        currentPage = 0;
        storyStarted = true;

        storyPanel.SetActive(true);

        if (continueHint != null)
            continueHint.SetActive(true);

        DisplayCurrentPage();

        Debug.Log("Story started.");
    }

    private void ShowNextPage()
    {
        storyPages[currentPage].SetActive(false);
        currentPage++;

        if (currentPage >= storyPages.Length)
        {
            FinishStory();
            return;
        }

        DisplayCurrentPage();
    }

    private void DisplayCurrentPage()
    {
        storyPages[currentPage].SetActive(true);
        Debug.Log($"Story page {currentPage + 1}/{storyPages.Length}");
    }

    private void FinishStory()
    {
        storyStarted = false;
        Time.timeScale = previousTimeScale;

        SceneManager.LoadScene(gameSceneName);
    }

    private void OnDisable()
    {
        if (!storyStarted)
            return;

        storyStarted = false;
        Time.timeScale = previousTimeScale;
    }
}
