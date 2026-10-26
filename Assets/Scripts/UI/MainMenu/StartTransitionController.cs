
using System.Collections;
using UnityEngine;

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

    [Header("Story")]
    [SerializeField] private StorySequenceManager storySequenceManager;

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
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopMusic();

        if (menuPanel != null)
            menuPanel.SetActive(false);

        if (transitionObject != null)
            transitionObject.SetActive(true);

        if (transitionAnimator != null)
        {
            transitionAnimator.Play(
                animationStateName,
                0,
                0f
            );
        }

        if (glitchSource != null && glitchSFX != null)
            glitchSource.PlayOneShot(glitchSFX);

        if (dialogueSource != null && characterDialogue != null)
        {
            dialogueSource.clip = characterDialogue;
            dialogueSource.Play();
        }

        StartCoroutine(TriggerUIShake());

        if (dialogueSource != null && characterDialogue != null)
        {
            yield return new WaitWhile(
                () => dialogueSource.isPlaying
            );
        }

        if (transitionObject != null)
            transitionObject.SetActive(false);

        if (storySequenceManager != null)
        {
            storySequenceManager.BeginStory();
        }
        else
        {
            Debug.LogError(
                "Story Sequence Manager is not assigned!",
                this
            );
        }
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
