using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DarknessProgressionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform sachikoTarget;
    [SerializeField] private Image darknessImage;

    [Header("Circle Radius")]
    [SerializeField, Range(0.1f, 1.5f)] private float wave1Radius = 0.85f;
    [SerializeField, Range(0.05f, 1f)] private float finalWaveRadius = 0.25f;
    [SerializeField] private float minimumRadius = 0.05f;

    [Header("Circle Movement")]
    [SerializeField] private float circleMoveSpeed = 0.6f;
    [SerializeField] private float minMoveInterval = 7f;
    [SerializeField] private float maxMoveInterval = 12f;
    [SerializeField] private float maxDriftFromStart = 3f;

    [Header("Red Tint")]
    [SerializeField, Range(0f, 1f)] private float startingRedTint = 0.05f;
    [SerializeField, Range(0f, 1f)] private float finalRedTint = 0.7f;
    [SerializeField, Range(0f, 1f)] private float startingEdgeGlow = 0.15f;
    [SerializeField, Range(0f, 1f)] private float finalEdgeGlow = 0.8f;
    [SerializeField, Range(0f, 1f)] private float outsideOpacity = 0.35f;

    [Header("Progression")]
    [SerializeField] private float transitionSpeed = 1.5f;

    private Material darknessMaterial;
    private RectTransform darknessRect;

    private Vector2 safeZoneCenter;
    private Vector2 startingCenter;
    private Vector2 movementTarget;

    private float currentRadius;
    private float targetRadius;
    private float movementTimer;

    private bool effectEnabled = true;
    private bool endingTransition;
    private int lastWave = -1;

    public float CurrentRadius => currentRadius;
    public Vector2 SafeZoneCenter => safeZoneCenter;

    public float SafeZoneRadiusWorld
    {
        get
        {
            Camera cam = Camera.main;

            if (cam == null || !cam.orthographic)
                return currentRadius;

            return currentRadius * cam.orthographicSize * 2f;
        }
    }

    private void Start()
    {
        if (waveManager == null)
            waveManager = FindFirstObjectByType<WaveManager>();

        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();

        if (sachikoTarget == null && playerController != null)
            sachikoTarget = playerController.transform;

        if (waveManager == null || sachikoTarget == null || darknessImage == null)
        {
            Debug.LogError("DarknessProgressionManager is missing a required reference.");
            enabled = false;
            return;
        }

        if (darknessImage.material == null ||
            !darknessImage.material.HasProperty("_Radius"))
        {
            Debug.LogError("Assign a material using the Sachiko/BlackIris shader.");
            enabled = false;
            return;
        }

        darknessMaterial = new Material(darknessImage.material);
        darknessImage.material = darknessMaterial;
        darknessRect = darknessImage.rectTransform;

        float aspect = darknessRect.rect.height > 0f
            ? darknessRect.rect.width / darknessRect.rect.height
            : 16f / 9f;

        darknessMaterial.SetFloat("_Aspect", aspect);
        darknessMaterial.SetFloat("_Alpha", 1f);
        darknessMaterial.SetFloat("_Opacity", outsideOpacity);

        darknessImage.gameObject.SetActive(true);

        safeZoneCenter = sachikoTarget.position;
        startingCenter = safeZoneCenter;
        movementTarget = safeZoneCenter;

        targetRadius = GetRadiusForWave(waveManager.CurrentWave);
        currentRadius = targetRadius;
        lastWave = waveManager.CurrentWave;

        ChooseNewMovementTarget();
        UpdateProgression(true);
        UpdateCenter();
    }

    private void Update()
    {
        if (!effectEnabled || endingTransition || waveManager == null)
            return;

        if (waveManager.CurrentWave != lastWave)
        {
            lastWave = waveManager.CurrentWave;
            targetRadius = GetRadiusForWave(lastWave);
        }

        UpdateCircleMovement();
        UpdateProgression(false);
        UpdateCenter();
    }

    private float GetRadiusForWave(int wave)
    {
        int totalWaves = Mathf.Max(1, waveManager.FinalWave);

        float progress = totalWaves <= 1
            ? 1f
            : Mathf.Clamp01((wave - 1f) / (totalWaves - 1f));

        return Mathf.Lerp(wave1Radius, finalWaveRadius, progress);
    }

    private void UpdateCircleMovement()
    {
        movementTimer -= Time.deltaTime;

        if (movementTimer <= 0f ||
            Vector2.Distance(safeZoneCenter, movementTarget) < 0.1f)
        {
            ChooseNewMovementTarget();
        }

        safeZoneCenter = Vector2.MoveTowards(
            safeZoneCenter,
            movementTarget,
            circleMoveSpeed * Time.deltaTime
        );
    }

    private void ChooseNewMovementTarget()
    {
        Vector2 randomOffset = Random.insideUnitCircle * maxDriftFromStart;

        movementTarget = startingCenter + randomOffset;

        movementTimer = Random.Range(
            minMoveInterval,
            Mathf.Max(minMoveInterval, maxMoveInterval)
        );
    }

    private void UpdateProgression(bool instant)
    {
        float progress = Mathf.InverseLerp(
            wave1Radius,
            finalWaveRadius,
            currentRadius
        );

        float waveProgress = waveManager.FinalWave <= 1
            ? 1f
            : Mathf.Clamp01(
                (waveManager.CurrentWave - 1f) /
                (waveManager.FinalWave - 1f)
            );

        float redTint = Mathf.Lerp(startingRedTint, finalRedTint, waveProgress);
        float edgeGlow = Mathf.Lerp(startingEdgeGlow, finalEdgeGlow, waveProgress);

        currentRadius = instant
            ? targetRadius
            : Mathf.Lerp(
                currentRadius,
                targetRadius,
                Time.deltaTime * transitionSpeed
            );

        darknessMaterial.SetFloat("_Radius", currentRadius);
        darknessMaterial.SetFloat("_RedTint", redTint);
        darknessMaterial.SetFloat("_EdgeGlow", edgeGlow);
        darknessMaterial.SetFloat("_Opacity", outsideOpacity);
        darknessMaterial.SetFloat("_Alpha", 1f);
    }

    private void UpdateCenter()
    {
        if (darknessRect == null)
            return;

        Camera cam = Camera.main;

        if (cam == null)
            return;

        Vector3 screenPosition = cam.WorldToScreenPoint(safeZoneCenter);

        if (screenPosition.z < 0f)
            return;

        Canvas canvas = darknessImage.canvas;
        Camera uiCamera = null;

        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = canvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            darknessRect,
            screenPosition,
            uiCamera,
            out Vector2 localPoint))
            return;

        Rect rect = darknessRect.rect;

        float u = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
        float v = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);

        darknessMaterial.SetVector("_Center", new Vector4(u, v, 0f, 0f));
    }

    public bool IsInsideSafeZone(Vector2 position)
    {
        float radius = SafeZoneRadiusWorld;
        return (position - safeZoneCenter).sqrMagnitude <= radius * radius;
    }

    public Vector2 ClampToSafeZone(Vector2 position)
    {
        Vector2 offset = position - safeZoneCenter;
        float radius = SafeZoneRadiusWorld;

        if (offset.sqrMagnitude <= radius * radius)
            return position;

        return safeZoneCenter + offset.normalized * radius;
    }

    public void SetEffectEnabled(bool value)
    {
        effectEnabled = value;

        if (darknessImage != null)
            darknessImage.gameObject.SetActive(value);
    }

    public void CollapseCircle(float duration)
    {
        StopAllCoroutines();
        endingTransition = true;
        StartCoroutine(CollapseRoutine(duration));
    }

    private IEnumerator CollapseRoutine(float duration)
    {
        float startRadius = currentRadius;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = duration > 0f
                ? Mathf.Clamp01(elapsed / duration)
                : 1f;

            currentRadius = Mathf.Lerp(startRadius, minimumRadius, t);
            darknessMaterial.SetFloat("_Radius", currentRadius);

            yield return null;
        }

        currentRadius = minimumRadius;
        darknessMaterial.SetFloat("_Radius", minimumRadius);
    }

    public void CollapseToBlack(float duration)
    {
        if (darknessMaterial == null || darknessImage == null)
        {
            Debug.LogError("Cannot collapse darkness: material or image is missing.");
            return;
        }

        StopAllCoroutines();

        effectEnabled = true;
        endingTransition = true;
        darknessImage.gameObject.SetActive(true);

        UpdateCenter();

        StartCoroutine(CollapseToBlackRoutine(duration));
    }

    private IEnumerator CollapseToBlackRoutine(float duration)
    {
        float startRadius = currentRadius;
        float startOpacity = darknessMaterial.GetFloat("_Opacity");
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = duration > 0f
                ? Mathf.Clamp01(elapsed / duration)
                : 1f;

            t = 1f - Mathf.Pow(1f - t, 3f);

            currentRadius = Mathf.Lerp(startRadius, 0f, t);

            darknessMaterial.SetFloat("_Radius", currentRadius);
            darknessMaterial.SetFloat("_Opacity", Mathf.Lerp(startOpacity, 1f, t));
            darknessMaterial.SetFloat("_Alpha", 1f);

            yield return null;
        }

        currentRadius = 0f;

        darknessMaterial.SetFloat("_Radius", 0f);
        darknessMaterial.SetFloat("_Opacity", 1f);
        darknessMaterial.SetFloat("_Alpha", 1f);
    }

    private void OnDestroy()
    {
        if (darknessMaterial != null)
            Destroy(darknessMaterial);
    }
}