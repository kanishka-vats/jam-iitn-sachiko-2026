using System.Collections;
using UnityEngine;

public class UIShake : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
    }

    public void Shake(float duration, float strength)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        shakeCoroutine = StartCoroutine(
            ShakeRoutine(duration, strength)
        );
    }

    private IEnumerator ShakeRoutine(
        float duration,
        float strength)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Vector2 offset =
                Random.insideUnitCircle * strength;

            rectTransform.anchoredPosition =
                originalPosition + offset;

            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        rectTransform.anchoredPosition = originalPosition;

        shakeCoroutine = null;
    }
}