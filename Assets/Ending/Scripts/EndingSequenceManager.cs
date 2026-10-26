
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class EndingSequenceManager : MonoBehaviour
{
    [Header("Ending Overlay")]
    [SerializeField] private GameObject endingOverlay;

    [Header("Victory")]
    [SerializeField] private GameObject victoryPanel;

    [Header("Character Focus")]
    [SerializeField] private Transform sachikoTarget;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private DarknessProgressionManager darknessProgressionManager;

    [Header("Ending UI")]
    [SerializeField] private Image irisImage;
    [SerializeField] private Image glitchImage;
    [SerializeField] private Image whiteFlash;
    [SerializeField] private Image awakeningImage;

    [Header("Timing")]
    [SerializeField] private float irisCloseDuration = 2.5f;
    [SerializeField] private float glitchDuration = 0.45f;
    [SerializeField] private float flashInDuration = 0.04f;
    [SerializeField] private float flashHoldDuration = 0.12f;
    [SerializeField] private float flashFadeDuration = 0.6f;
    [SerializeField] private float awakeningFadeDuration = 1.5f;

    [Header("Glitch Settings")]
    [SerializeField, Range(0f, 2f)] private float glitchIntensity = 1.3f;
    [SerializeField] private float glitchPeakDuration = 0.12f;
    [SerializeField] private float glitchSpeed = 1.5f;

    [Header("Camera Shake")]
    [SerializeField] private Transform cameraShakeTarget;
    [SerializeField] private float cameraShakeDuration = 0.3f;
    [SerializeField] private float cameraShakeMagnitude = 0.1f;

    private Material irisMaterial;
    private Material glitchMaterial;
    private bool endingStarted;

    private void Start()
    {
        if (darknessProgressionManager == null)
            darknessProgressionManager = FindFirstObjectByType<DarknessProgressionManager>();

        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();

        if (sachikoTarget == null && playerController != null)
            sachikoTarget = playerController.transform;

        SetupIris();
        SetupGlitch();

        SetImageAlpha(whiteFlash, 0f);
        SetImageAlpha(awakeningImage, 0f);

        if (endingOverlay != null)
            endingOverlay.SetActive(false);

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        Debug.Log("EndingSequenceManager ready.");
    }

    private void SetupIris()
    {
        if (irisImage == null || irisImage.material == null)
            return;

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
        SetImageAlpha(irisImage, 0f);
    }

    private void SetupGlitch()
    {
        if (glitchImage == null)
            return;

        if (glitchImage.material == null ||
            !glitchImage.material.HasProperty("_Progress") ||
            !glitchImage.material.HasProperty("_Intensity") ||
            !glitchImage.material.HasProperty("_Fade"))
        {
            Debug.LogError("Assign a material using the Sachiko/Glitch shader to Glitch Image.");
            glitchImage.gameObject.SetActive(false);
            return;
        }

        glitchMaterial = new Material(glitchImage.material);
        glitchImage.material = glitchMaterial;

        glitchMaterial.SetFloat("_Progress", 0f);
        glitchMaterial.SetFloat("_Intensity", glitchIntensity);
        glitchMaterial.SetFloat("_Fade", 0f);
        glitchMaterial.SetFloat("_Speed", glitchSpeed);

        glitchImage.color = Color.white;
        glitchImage.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (endingStarted)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartCoroutine(PlayEnding(false));
        }
    }

    public void PlayVictoryEnding()
    {
        if (endingStarted)
            return;

        StartCoroutine(PlayEnding(true));
    }

    private IEnumerator PlayEnding(bool useGameplayDarkness)
    {
        endingStarted = true;

        if (playerController != null)
            playerController.SetMovementEnabled(false);

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (endingOverlay != null)
            endingOverlay.SetActive(true);

        if (useGameplayDarkness && darknessProgressionManager != null)
        {
            darknessProgressionManager.CollapseToBlack(irisCloseDuration);
            yield return new WaitForSecondsRealtime(irisCloseDuration);
        }
        else
        {
            if (darknessProgressionManager != null)
                darknessProgressionManager.SetEffectEnabled(false);

            if (irisImage != null && irisMaterial != null)
            {
                irisImage.gameObject.SetActive(true);
                irisImage.transform.SetAsLastSibling();

                SetIrisCenter(GetSachikoScreenPosition(irisImage));
                SetImageAlpha(irisImage, 1f);
                irisMaterial.SetFloat("_Alpha", 1f);

                yield return AnimateIris(1.5f, 0.05f, irisCloseDuration);
            }
        }

        yield return PlayGlitch();

        if (whiteFlash != null)
        {
            whiteFlash.gameObject.SetActive(true);
            whiteFlash.transform.SetAsLastSibling();

            yield return FadeImage(whiteFlash, 0f, 1f, flashInDuration);
            yield return new WaitForSecondsRealtime(flashHoldDuration);

            SetImageAlpha(irisImage, 0f);

            if (darknessProgressionManager != null)
                darknessProgressionManager.SetEffectEnabled(false);

            yield return FadeImage(whiteFlash, 1f, 0f, flashFadeDuration);
            whiteFlash.gameObject.SetActive(false);
        }
        else
        {
            SetImageAlpha(irisImage, 0f);

            if (darknessProgressionManager != null)
                darknessProgressionManager.SetEffectEnabled(false);
        }

        if (awakeningImage != null && awakeningImage.sprite != null)
        {
            awakeningImage.gameObject.SetActive(true);
            awakeningImage.transform.SetAsLastSibling();

            yield return FadeImage(
                awakeningImage,
                0f,
                1f,
                awakeningFadeDuration
            );
        }

        if (endingOverlay != null)
            endingOverlay.SetActive(false);

        if (victoryPanel != null)
            victoryPanel.SetActive(true);
        else
            Debug.LogError("EndingSequenceManager: Victory Panel is not assigned.");

        Debug.Log("Victory ending completed.");
    }

    private IEnumerator PlayGlitch()
    {
        if (glitchImage == null || glitchMaterial == null)
        {
            yield return ShakeCamera();
            yield break;
        }

        glitchImage.gameObject.SetActive(true);
        glitchImage.color = Color.white;
        glitchImage.transform.SetAsLastSibling();

        glitchMaterial.SetFloat("_Progress", 0f);
        glitchMaterial.SetFloat("_Intensity", glitchIntensity);
        glitchMaterial.SetFloat("_Fade", 1f);
        glitchMaterial.SetFloat("_Speed", glitchSpeed);

        StartCoroutine(ShakeCamera());

        float elapsed = 0f;

        while (elapsed < glitchDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / glitchDuration);
            float progress = Mathf.SmoothStep(0f, 1f, t);

            float intensity = t < 0.35f
                ? Mathf.Lerp(0.2f, glitchIntensity, t / 0.35f)
                : Mathf.Lerp(
                    glitchIntensity,
                    glitchIntensity * 0.65f,
                    Mathf.Clamp01((t - 0.35f) / 0.65f)
                );

            if (t < glitchPeakDuration / Mathf.Max(glitchDuration, 0.001f))
                intensity = glitchIntensity * 1.4f;

            glitchMaterial.SetFloat("_Progress", progress);
            glitchMaterial.SetFloat("_Intensity", intensity);
            glitchMaterial.SetFloat("_Fade", 1f);

            yield return null;
        }

        glitchMaterial.SetFloat("_Progress", 1f);
        glitchMaterial.SetFloat("_Fade", 0f);
        glitchImage.gameObject.SetActive(false);
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

    private Vector2 GetSachikoScreenPosition(Image image = null)
    {
        if (sachikoTarget == null || Camera.main == null)
            return new Vector2(0.5f, 0.5f);

        Vector3 screenPosition =
            Camera.main.WorldToScreenPoint(sachikoTarget.position);

        if (screenPosition.z < 0f)
            return new Vector2(0.5f, 0.5f);

        if (image == null)
            image = irisImage;

        if (image == null)
            return new Vector2(0.5f, 0.5f);

        Canvas canvas = image.canvas;
        Camera uiCamera = null;

        if (canvas != null &&
            canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = canvas.worldCamera;

        RectTransform rect = image.rectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect,
            screenPosition,
            uiCamera,
            out Vector2 localPoint))
        {
            Rect bounds = rect.rect;

            float u = Mathf.InverseLerp(
                bounds.xMin,
                bounds.xMax,
                localPoint.x
            );

            float v = Mathf.InverseLerp(
                bounds.yMin,
                bounds.yMax,
                localPoint.y
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

    private void OnDestroy()
    {
        if (irisMaterial != null)
            Destroy(irisMaterial);

        if (glitchMaterial != null)
            Destroy(glitchMaterial);
    }
}
