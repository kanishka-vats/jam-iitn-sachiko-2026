using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHoverEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Hover Movement")]
    [SerializeField] private float hoverMoveY = 6f;

    [Header("Hover Scale")]
    [SerializeField] private float hoverScale = 1.05f;

    [Header("Hover Rotation")]
    [SerializeField] private float hoverRotation = 1.5f;

    [Header("Animation")]
    [SerializeField] private float animationSpeed = 12f;

    [Header("Optional")]
    [SerializeField] private Image glow;

    private RectTransform rectTransform;

    private Vector2 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    private Vector2 targetPosition;
    private Vector3 targetScale;
    private Quaternion targetRotation;

    private bool hovering;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        originalPosition = rectTransform.anchoredPosition;
        originalScale = rectTransform.localScale;
        originalRotation = rectTransform.localRotation;

        targetPosition = originalPosition;
        targetScale = originalScale;
        targetRotation = originalRotation;

        if (glow != null)
            glow.enabled = false;
    }

    private void Update()
    {
        rectTransform.anchoredPosition = Vector2.Lerp(
            rectTransform.anchoredPosition,
            targetPosition,
            Time.unscaledDeltaTime * animationSpeed
        );

        rectTransform.localScale = Vector3.Lerp(
            rectTransform.localScale,
            targetScale,
            Time.unscaledDeltaTime * animationSpeed
        );

        rectTransform.localRotation = Quaternion.Lerp(
            rectTransform.localRotation,
            targetRotation,
            Time.unscaledDeltaTime * animationSpeed
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;

        targetPosition = originalPosition + Vector2.up * hoverMoveY;

        targetScale = originalScale * hoverScale;

        targetRotation = Quaternion.Euler(
            0f,
            0f,
            hoverRotation
        );

        if (glow != null)
            glow.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;

        targetPosition = originalPosition;
        targetScale = originalScale;
        targetRotation = originalRotation;

        if (glow != null)
            glow.enabled = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = originalScale * 0.97f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (hovering)
        {
            targetScale = originalScale * hoverScale;
        }
    }
}