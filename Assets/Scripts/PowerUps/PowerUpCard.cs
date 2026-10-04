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
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private TMP_Text costText;

    [Header("Hover Settings")]
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float animationSpeed = 10f;

    [Header("Purchase Visual")]
    [SerializeField] private float unavailableAlpha = 0.75f;

    private PowerUpData powerUpData;
    private PowerUpManager powerUpManager;

    private Vector3 originalScale;
    private bool isHovered;

    private Image cardImage;

    private void Awake()
    {
        originalScale = transform.localScale;

        // Get the Image component from the PowerUpCard itself
        cardImage = GetComponent<Image>();

        if (powerUpInfo != null)
        {
            powerUpInfo.SetActive(false);
        }
    }

    private void Update()
    {
        Vector3 targetScale = isHovered
            ? originalScale * hoverScale
            : originalScale;

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            animationSpeed * Time.deltaTime
        );
    }

    public void Setup(
        PowerUpData data,
        PowerUpManager manager
    )
    {
        powerUpData = data;
        powerUpManager = manager;

        // Apply the artwork from the selected PowerUpData
        if (cardImage != null && data.cardImage != null)
        {
            cardImage.sprite = data.cardImage;
        }

        if (infoText != null)
        {
            infoText.text = data.description;
        }

        if (costText != null)
        {
            costText.text = "💎 " + data.diamondCost;
        }

        if (powerUpInfo != null)
        {
            powerUpInfo.SetActive(false);
        }

        UpdatePurchaseState();
    }

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

        if (cardImage != null)
        {
            Color color = cardImage.color;
            color.a = canAfford ? 1f : unavailableAlpha;
            cardImage.color = color;
        }
    }

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

    public void OnPointerClick(
        PointerEventData eventData
    )
    {
        if (powerUpData == null ||
            powerUpManager == null)
        {
            return;
        }

        // Don't allow unaffordable purchases
        if (CurrencyManager.Instance == null ||
            !CurrencyManager.Instance.CanAfford(
                powerUpData.diamondCost
            ))
        {
            return;
        }

        powerUpManager.TryPurchasePowerUp(
            powerUpData
        );
    }

    public PowerUpData GetPowerUpData()
    {
        return powerUpData;
    }
}