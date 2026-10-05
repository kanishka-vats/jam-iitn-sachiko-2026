using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIFrameAnimation : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite[] frames;

    [SerializeField] private float frameRate = 0.12f;

    private Coroutine animationCoroutine;

    public void Play()
    {
        Stop();

        if (targetImage == null)
        {
            Debug.LogWarning(
                "UIFrameAnimation: Target Image is not assigned."
            );

            return;
        }

        if (frames == null || frames.Length == 0)
        {
            Debug.LogWarning(
                "UIFrameAnimation: No animation frames assigned."
            );

            return;
        }

        animationCoroutine =
            StartCoroutine(
                PlayAnimation()
            );
    }

    public void Stop()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(
                animationCoroutine
            );

            animationCoroutine = null;
        }
    }

    private IEnumerator PlayAnimation()
    {
        int frameIndex = 0;

        while (true)
        {
            targetImage.sprite =
                frames[frameIndex];

            frameIndex++;

            if (frameIndex >= frames.Length)
            {
                frameIndex = 0;
            }

            // IMPORTANT:
            // Realtime means this animation continues
            // normally even when Time.timeScale = 0.2.
            yield return new WaitForSecondsRealtime(
                frameRate
            );
        }
    }

    private void OnDisable()
    {
        Stop();
    }
}