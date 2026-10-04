using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartTransitionController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject transitionObject;

    [Header("Animation")]
    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private string animationStateName = "StartMenuUI";

    [Header("Audio")]
    [SerializeField] private AudioSource dialogueSource;
    [SerializeField] private AudioClip characterDialogue;

    [SerializeField] private AudioSource glitchSource;
    [SerializeField] private AudioClip glitchSFX;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("UI Shake")]
    [SerializeField] private UIShake uiShake;
    [SerializeField] private float shakeDelay = 0.5f;
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeStrength = 15f;

    private bool isTransitioning;

    public void StartGame()
    {
        if (isTransitioning)
            return;

        isTransitioning = true;

        StartCoroutine(StartTransition());
    }

    private IEnumerator StartTransition()
    {
        // Hide menu
        if (menuPanel != null)
            menuPanel.SetActive(false);

        // Show transition animation
        if (transitionObject != null)
            transitionObject.SetActive(true);

        // Play animation from beginning
        if (transitionAnimator != null)
        {
            transitionAnimator.Play(
                animationStateName,
                0,
                0f
            );
        }

        // Play glitch SFX
        if (glitchSource != null && glitchSFX != null)
        {
            glitchSource.PlayOneShot(glitchSFX);
        }

        // Play character dialogue
        if (dialogueSource != null && characterDialogue != null)
        {
            dialogueSource.clip = characterDialogue;
            dialogueSource.Play();
        }

        // Trigger UI shake
        StartCoroutine(TriggerUIShake());

        // Wait until dialogue finishes
        if (dialogueSource != null && characterDialogue != null)
        {
            yield return new WaitWhile(
                () => dialogueSource.isPlaying
            );
        }

        // Dialogue finished -> immediately load game
        SceneManager.LoadScene(gameSceneName);
    }

    private IEnumerator TriggerUIShake()
    {
        if (uiShake == null)
            yield break;

        yield return new WaitForSecondsRealtime(shakeDelay);

        uiShake.Shake(
            shakeDuration,
            shakeStrength
        );
    }
}