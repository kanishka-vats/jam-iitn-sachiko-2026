
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class EndingSequenceManager : MonoBehaviour
{
    [Header("Ending Overlay")]
    [SerializeField] private GameObject endingOverlay;

    [Header("Character Focus")]
    [SerializeField] private Transform sachikoTarget;
    [SerializeField] private PlayerController playerController;

    [Header("Ending UI References")]
    [SerializeField] private Image irisImage;
    [SerializeField] private Image explosionImage;
    [SerializeField] private Image whiteFlash;
    [SerializeField] private Image awakeningImage;

    [Header("Timing")]
    [SerializeField] private float irisCloseDuration = 2.5f;
    [SerializeField] private float explosionDuration = 0.5f;
    [SerializeField] private float flashHoldDuration = 0.12f;
    [SerializeField] private float flashFadeDuration = 0.5f;
    [SerializeField] private float awakeningFadeDuration = 1.5f;

    [Header("Camera Shake")]
    [SerializeField] private Transform cameraShakeTarget;
    [SerializeField] private float cameraShakeDuration = 0.18f;
    [SerializeField] private float cameraShakeMagnitude = 0.08f;

    [Header("Explosion Animation")]
    [SerializeField] private float explosionExpandDuration = 0.18f;
    [SerializeField] private float explosionStartScale = 0.08f;
    [SerializeField] private float explosionEndScale = 2f;

    private Material irisMaterial;
    private bool endingStarted;

    private Sprite generatedFlashSprite;
    private Sprite generatedExplosionSprite;
    private Texture2D generatedFlashTexture;
    private Texture2D generatedExplosionTexture;

    private void Start()
    {
        if (irisImage != null && irisImage.material != null)
        {
            irisMaterial = new Material(irisImage.material);
            irisImage.material = irisMaterial;

            RectTransform rect = irisImage.rectTransform;

            float aspect = rect.rect.height > 0f
                ? rect.rect.width / rect.rect.height
                : 16f / 9f;

            irisMaterial.SetFloat("_Aspect", aspect);
            irisMaterial.SetFloat("_Radius", 1.5f);
            irisMaterial.SetFloat("_Alpha", 0f);

            SetIrisCenter(new Vector2(0.5f, 0.5f));
        }

        if (explosionImage != null && explosionImage.sprite == null)
            CreateExplosionSprite();

        if (whiteFlash != null && whiteFlash.sprite == null)
            CreateFlashSprite();

        if (whiteFlash != null)
            whiteFlash.color = Color.white;

        if (explosionImage != null)
            explosionImage.color = new Color(1f, 0.25f, 0.03f, 0f);

        SetImageAlpha(irisImage, 0f);
        SetImageAlpha(explosionImage, 0f);
        SetImageAlpha(whiteFlash, 0f);
        SetImageAlpha(awakeningImage, 0f);

        if (endingOverlay != null)
            endingOverlay.SetActive(false);

        Debug.Log("EndingSequenceManager ready. Press E to test.");
    }

    private void Update()
    {
        if (endingStarted)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartCoroutine(PlayEnding());
        }
    }

    private IEnumerator PlayEnding()
    {
        endingStarted = true;

        if (playerController != null)
            playerController.SetMovementEnabled(false);

        yield return new WaitForFixedUpdate();

        if (endingOverlay != null)
            endingOverlay.SetActive(true);

        if (irisImage != null && irisMaterial != null)
        {
            irisImage.transform.SetAsFirstSibling();

            SetIrisCenter(GetSachikoScreenPosition());
            SetImageAlpha(irisImage, 1f);
            irisMaterial.SetFloat("_Alpha", 1f);

            yield return AnimateIris(1.5f, 0.10f, irisCloseDuration);
        }

        yield return ShakeCamera();

        yield return new WaitForSecondsRealtime(0.08f);

        if (explosionImage != null)
        {
            explosionImage.transform.SetAsLastSibling();

            yield return AnimateExplosion();

            float remainingTime = Mathf.Max(
                0f,
                explosionDuration - explosionExpandDuration
            );

            yield return new WaitForSecondsRealtime(remainingTime);
        }

        if (whiteFlash != null)
        {
            whiteFlash.transform.SetAsLastSibling();

            yield return FadeImage(whiteFlash, 0f, 1f, 0.03f);

            yield return new WaitForSecondsRealtime(flashHoldDuration);

            SetImageAlpha(irisImage, 0f);
            SetImageAlpha(explosionImage, 0f);

            yield return FadeImage(
                whiteFlash, 1f, 0f, flashFadeDuration
            );
        }

        if (awakeningImage != null && awakeningImage.sprite != null)
        {
            awakeningImage.transform.SetAsLastSibling();

            yield return FadeImage(
                awakeningImage, 0f, 1f, awakeningFadeDuration
            );
        }

        Debug.Log("Ending sequence completed.");
    }

    private IEnumerator ShakeCamera()
    {
        Transform target = cameraShakeTarget;

        if (target == null && Camera.main != null)
            target = Camera.main.transform;

        if (target == null || cameraShakeDuration <= 0f)
            yield break;

        Vector3 originalPosition = target.localPosition;
        float elapsed = 0f;

        while (elapsed < cameraShakeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float strength = 1f - Mathf.Clamp01(
                elapsed / cameraShakeDuration
            );

            Vector2 offset = Random.insideUnitCircle
                * cameraShakeMagnitude * strength;

            target.localPosition = originalPosition
                + new Vector3(offset.x, offset.y, 0f);

            yield return null;
        }

        target.localPosition = originalPosition;
    }

    private Vector2 GetSachikoScreenPosition()
    {
        if (sachikoTarget == null || Camera.main == null)
            return new Vector2(0.5f, 0.5f);

        Vector3 screenPosition =
            Camera.main.WorldToScreenPoint(sachikoTarget.position);

        if (screenPosition.z < 0f)
            return new Vector2(0.5f, 0.5f);

        Canvas canvas = irisImage.canvas;
        Camera uiCamera = null;

        if (canvas != null &&
            canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera;
        }

        RectTransform irisRect = irisImage.rectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            irisRect,
            screenPosition,
            uiCamera,
            out Vector2 localPoint))
        {
            Rect rect = irisRect.rect;

            float u = Mathf.InverseLerp(
                rect.xMin, rect.xMax, localPoint.x
            );

            float v = Mathf.InverseLerp(
                rect.yMin, rect.yMax, localPoint.y
            );

            return new Vector2(u, v);
        }

        return new Vector2(0.5f, 0.5f);
    }

    private void SetIrisCenter(Vector2 center)
    {
        if (irisMaterial != null &&
            irisMaterial.HasProperty("_Center"))
        {
            irisMaterial.SetVector(
                "_Center",
                new Vector4(center.x, center.y, 0f, 0f)
            );
        }
    }

    private IEnumerator AnimateIris(
        float startRadius,
        float endRadius,
        float duration)
    {
        if (irisMaterial == null)
            yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = duration > 0f
                ? Mathf.Clamp01(elapsed / duration)
                : 1f;

            t = 1f - Mathf.Pow(1f - t, 3f);

            irisMaterial.SetFloat(
                "_Radius",
                Mathf.Lerp(startRadius, endRadius, t)
            );

            yield return null;
        }

        irisMaterial.SetFloat("_Radius", endRadius);
    }

    private IEnumerator AnimateExplosion()
    {
        if (explosionImage == null)
            yield break;

        RectTransform rect = explosionImage.rectTransform;

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one * explosionStartScale;

        explosionImage.preserveAspect = false;
        SetImageAlpha(explosionImage, 1f);

        float elapsed = 0f;

        while (elapsed < explosionExpandDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = explosionExpandDuration > 0f
                ? Mathf.Clamp01(elapsed / explosionExpandDuration)
                : 1f;

            t = 1f - Mathf.Pow(1f - t, 3f);

            float scale = Mathf.Lerp(
                explosionStartScale,
                explosionEndScale,
                t
            );

            rect.localScale = Vector3.one * scale;

            yield return null;
        }

        rect.localScale = Vector3.one * explosionEndScale;
    }

    private IEnumerator FadeImage(
        Image image,
        float startAlpha,
        float endAlpha,
        float duration)
    {
        if (image == null)
            yield break;

        SetImageAlpha(image, startAlpha);

        if (duration <= 0f)
        {
            SetImageAlpha(image, endAlpha);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            SetImageAlpha(
                image,
                Mathf.Lerp(startAlpha, endAlpha, t)
            );

            yield return null;
        }

        SetImageAlpha(image, endAlpha);
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        if (image == null)
            return;

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    private void CreateExplosionSprite()
    {
        const int size = 256;

        generatedExplosionTexture = new Texture2D(
            size, size, TextureFormat.RGBA32, false
        );

        generatedExplosionTexture.name = "GeneratedEndingExplosion";
        generatedExplosionTexture.filterMode = FilterMode.Bilinear;
        generatedExplosionTexture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = (x + 0.5f) / size * 2f - 1f;
                float py = (y + 0.5f) / size * 2f - 1f;

                float radius = Mathf.Sqrt(px * px + py * py);
                float angle = Mathf.Atan2(py, px);

                float rayPattern =
                    0.5f + 0.5f * Mathf.Cos(angle * 18f);

                float rays = Mathf.Pow(rayPattern, 7f);

                float core =
                    1f - Mathf.SmoothStep(0f, 0.48f, radius);

                float rayLength = 0.58f + rays * 0.42f;

                float rayAlpha = 1f - Mathf.SmoothStep(
                    rayLength - 0.08f,
                    rayLength + 0.08f,
                    radius
                );

                float outerFade =
                    1f - Mathf.SmoothStep(0.85f, 1f, radius);

                float alpha = Mathf.Clamp01(
                    Mathf.Max(core, rayAlpha * rays) * outerFade
                );

                if (radius > 1f)
                    alpha = 0f;

                generatedExplosionTexture.SetPixel(
                    x, y, new Color(1f, 1f, 1f, alpha)
                );
            }
        }

        generatedExplosionTexture.Apply();

        generatedExplosionSprite = Sprite.Create(
            generatedExplosionTexture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f)
        );

        generatedExplosionSprite.name = "GeneratedEndingExplosion";
        explosionImage.sprite = generatedExplosionSprite;
    }

    private void CreateFlashSprite()
    {
        generatedFlashTexture = new Texture2D(
            1, 1, TextureFormat.RGBA32, false
        );

        generatedFlashTexture.name = "GeneratedWhiteFlash";
        generatedFlashTexture.SetPixel(0, 0, Color.white);
        generatedFlashTexture.Apply();

        generatedFlashSprite = Sprite.Create(
            generatedFlashTexture,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f)
        );

        generatedFlashSprite.name = "GeneratedWhiteFlash";
        whiteFlash.sprite = generatedFlashSprite;
    }

    private void OnDestroy()
    {
        if (irisMaterial != null)
            Destroy(irisMaterial);

        if (generatedExplosionSprite != null)
            Destroy(generatedExplosionSprite);

        if (generatedFlashSprite != null)
            Destroy(generatedFlashSprite);

        if (generatedExplosionTexture != null)
            Destroy(generatedExplosionTexture);

        if (generatedFlashTexture != null)
            Destroy(generatedFlashTexture);
    }
}
