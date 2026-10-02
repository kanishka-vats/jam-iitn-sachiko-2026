using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

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

    private PowerUpData powerUpData;
    private PowerUpManager powerUpManager;

    private Vector3 originalScale;
    private bool isHovered;

    private void Awake()
    {
        originalScale = transform.localScale;

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

        powerUpManager.TryPurchasePowerUp(
            powerUpData
        );
    }

    public PowerUpData GetPowerUpData()
    {
        return powerUpData;
    }
}