using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class PowerUpCard : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [Header("UI")]
    [SerializeField] private GameObject powerUpInfo;

    [Header("Cost UI")]
    [SerializeField] private Image currencyIcon;
    [SerializeField] private TMP_Text costText;

    [Header("Hover Settings")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float hoverLift = 10f;
    [SerializeField] private float hoverRotation = 2f;
    [SerializeField] private float animationSpeed = 10f;

    [Header("Hover Visual")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.white;

    [Header("Purchase Visual")]
    [SerializeField] private float unavailableAlpha = 0.55f;

    private PowerUpData powerUpData;
    private PowerUpManager powerUpManager;

    private Image cardImage;

    private Vector3 originalScale;
    private Quaternion originalRotation;

    private bool isHovered;

    private void Awake()
    {
        cardImage = GetComponent<Image>();

        originalScale = transform.localScale;
        originalRotation = transform.localRotation;

        if (powerUpInfo != null)
        {
            powerUpInfo.SetActive(false);
        }
    }

    private void Update()
    {
        UpdateHoverAnimation();
    }

    // =========================================================
    // SETUP
    // =========================================================

    public void Setup(
        PowerUpData data,
        PowerUpManager manager
    )
    {
        powerUpData = data;
        powerUpManager = manager;

        if (data == null)
            return;

        // -----------------------------------------------------
        // Card Artwork
        // -----------------------------------------------------

        if (cardImage != null &&
            data.cardImage != null)
        {
            cardImage.sprite = data.cardImage;
        }

        // -----------------------------------------------------
        // Power-Up Info PNG
        // -----------------------------------------------------

        if (powerUpInfo != null)
        {
            Image infoImage =
                powerUpInfo.GetComponent<Image>();

            if (infoImage != null &&
                data.infoImage != null)
            {
                infoImage.sprite = data.infoImage;

                infoImage.preserveAspect = true;
            }

            powerUpInfo.SetActive(false);
        }

        // -----------------------------------------------------
        // Cost
        // -----------------------------------------------------

        if (costText != null)
        {
            costText.text =
                data.diamondCost.ToString();
        }

        // -----------------------------------------------------
        // Reset Hover
        // -----------------------------------------------------

        isHovered = false;

        transform.localScale = originalScale;
        transform.localRotation = originalRotation;

        if (cardImage != null)
        {
            cardImage.color = normalColor;
        }

        UpdatePurchaseState();
    }

    // =========================================================
    // HOVER ANIMATION
    // =========================================================

    private void UpdateHoverAnimation()
    {
        Vector3 targetScale =
            isHovered
                ? originalScale * hoverScale
                : originalScale;

        Quaternion targetRotation =
            isHovered
                ? originalRotation *
                  Quaternion.Euler(
                      0f,
                      0f,
                      hoverRotation
                  )
                : originalRotation;

        transform.localScale =
            Vector3.Lerp(
                transform.localScale,
                targetScale,
                animationSpeed *
                Time.deltaTime
            );

        transform.localRotation =
            Quaternion.Lerp(
                transform.localRotation,
                targetRotation,
                animationSpeed *
                Time.deltaTime
            );

        // -----------------------------------------------------
        // Card Brightness
        // -----------------------------------------------------

        if (cardImage != null)
        {
            Color targetColor =
                isHovered
                    ? hoverColor
                    : normalColor;

            cardImage.color =
                Color.Lerp(
                    cardImage.color,
                    targetColor,
                    animationSpeed *
                    Time.deltaTime
                );
        }
    }

    // =========================================================
    // PURCHASE STATE
    // =========================================================

    private void UpdatePurchaseState()
    {
        if (powerUpData == null ||
            CurrencyManager.Instance == null)
        {
            return;
        }

        bool canAfford =
            CurrencyManager.Instance.CanAfford(
                powerUpData.diamondCost
            );

        // Card transparency
        if (cardImage != null)
        {
            Color color =
                cardImage.color;

            color.a =
                canAfford
                    ? 1f
                    : unavailableAlpha;

            cardImage.color = color;
        }

        // Cost text transparency
        if (costText != null)
        {
            costText.alpha =
                canAfford
                    ? 1f
                    : 0.6f;
        }

        // Currency icon transparency
        if (currencyIcon != null)
        {
            Color iconColor =
                currencyIcon.color;

            iconColor.a =
                canAfford
                    ? 1f
                    : 0.6f;

            currencyIcon.color =
                iconColor;
        }
    }

    // =========================================================
    // POINTER ENTER
    // =========================================================

    public void OnPointerEnter(
        PointerEventData eventData
    )
    {
        isHovered = true;

        if (powerUpInfo != null)
        {
            powerUpInfo.SetActive(true);
        }
    }

    // =========================================================
    // POINTER EXIT
    // =========================================================

    public void OnPointerExit(
        PointerEventData eventData
    )
    {
        isHovered = false;

        if (powerUpInfo != null)
        {
            powerUpInfo.SetActive(false);
        }
    }

    // =========================================================
    // POINTER CLICK
    // =========================================================

    public void OnPointerClick(
        PointerEventData eventData
    )
    {
        if (powerUpData == null ||
            powerUpManager == null)
        {
            return;
        }

        if (CurrencyManager.Instance == null)
        {
            return;
        }

        if (!CurrencyManager.Instance.CanAfford(
                powerUpData.diamondCost))
        {
            return;
        }

        powerUpManager.TryPurchasePowerUp(
            powerUpData
        );
    }

    // =========================================================
    // PUBLIC ACCESS
    // =========================================================

    public PowerUpData GetPowerUpData()
    {
        return powerUpData;
    }
}