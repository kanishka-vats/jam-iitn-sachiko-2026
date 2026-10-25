
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
        // Create a private material so we don't modify the shared asset.
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

        // Generate an explosion sprite if one wasn't assigned.
        if (explosionImage != null && explosionImage.sprite == null)
            CreateExplosionSprite();

        // Generate a plain white sprite for the flash if needed.
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

        // Start with gameplay visible.
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

        // Freeze movement, cancel dash and stop shooting.
        if (playerController != null)
        {
            playerController.SetMovementEnabled(false);
            Debug.Log("ENDING: Sachiko frozen.");
        }
        else
        {
            Debug.LogError(
                "ENDING: PlayerController is not assigned!"
            );
        }

        // Allow the Rigidbody to settle before reading the position.
        yield return new WaitForFixedUpdate();

        if (endingOverlay != null)
            endingOverlay.SetActive(true);

        // PHASE 1: Close the iris around stationary Sachiko.
        Debug.Log("ENDING: Closing iris around Sachiko.");

        if (irisImage != null && irisMaterial != null)
        {
            irisImage.transform.SetAsFirstSibling();

            Vector2 center = GetSachikoScreenPosition();
            SetIrisCenter(center);

            Debug.Log($"ENDING: Iris center = {center}");

            SetImageAlpha(irisImage, 1f);
            irisMaterial.SetFloat("_Alpha", 1f);

            yield return AnimateIris(
                1.5f, 0.025f, irisCloseDuration
            );
        }
        else
        {
            Debug.LogError(
                "ENDING: Iris Image or its material is missing."
            );
        }

        // PHASE 2: Brief darkness before the blast.
        Debug.Log("ENDING: Darkness.");
        yield return new WaitForSecondsRealtime(0.25f);

        // PHASE 3: Expanding explosion.
        if (explosionImage != null)
        {
            Debug.Log("ENDING: Blast!");

            explosionImage.transform.SetAsLastSibling();

            yield return AnimateExplosion();

            float remainingTime = Mathf.Max(
                0f, explosionDuration - explosionExpandDuration
            );

            yield return new WaitForSecondsRealtime(remainingTime);
        }

        // PHASE 4: White flash.
        if (whiteFlash != null)
        {
            Debug.Log("ENDING: White flash!");

            whiteFlash.transform.SetAsLastSibling();

            yield return FadeImage(
                whiteFlash, 0f, 1f, 0.03f
            );

            yield return new WaitForSecondsRealtime(
                flashHoldDuration
            );

            // Remove the old effects while the screen is white.
            SetImageAlpha(irisImage, 0f);
            SetImageAlpha(explosionImage, 0f);

            yield return FadeImage(
                whiteFlash, 1f, 0f, flashFadeDuration
            );
        }

        // PHASE 5: Reveal the awakening artwork, if assigned.
        if (awakeningImage != null && awakeningImage.sprite != null)
        {
            Debug.Log("ENDING: Awakening reveal.");

            awakeningImage.transform.SetAsLastSibling();

            yield return FadeImage(
                awakeningImage, 0f, 1f, awakeningFadeDuration
            );
        }
        else
        {
            Debug.LogWarning(
                "No awakening sprite assigned. Skipping awakening reveal."
            );
        }

        Debug.Log("ENDING: Sequence completed.");
    }

    private Vector2 GetSachikoScreenPosition()
    {
        if (sachikoTarget == null)
        {
            Debug.LogError(
                "Sachiko Target is missing. Using screen centre."
            );

            return new Vector2(0.5f, 0.5f);
        }

        Camera worldCamera = Camera.main;

        if (worldCamera == null)
        {
            Debug.LogError("Main Camera not found.");
            return new Vector2(0.5f, 0.5f);
        }

        Vector3 screenPosition =
            worldCamera.WorldToScreenPoint(sachikoTarget.position);

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

            Debug.Log($"Sachiko iris UV: ({u:F2}, {v:F2})");

            return new Vector2(u, v);
        }

        Debug.LogWarning(
            "Could not convert Sachiko's position to UI coordinates."
        );

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

            t = t * t * (3f - 2f * t);

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

        // Stretch the explosion image across its parent Canvas.
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

            // Fast, dramatic expansion.
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
