using UnityEngine;
using UnityEngine.UI;

public class PowerUpCard : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image cardImage;
    [SerializeField] private Button selectButton;

    private PowerUpData powerUpData;
    private PowerUpManager powerUpManager;

    public void Setup(PowerUpData data, PowerUpManager manager)
    {
        powerUpData = data;
        powerUpManager = manager;

        if (cardImage != null)
        {
            cardImage.sprite = data.cardImage;
        }

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(SelectCard);
        }
    }

    private void SelectCard()
    {
        if (powerUpData == null || powerUpManager == null)
            return;

        powerUpManager.SelectPowerUp(powerUpData);

        Debug.Log(
            "Selected Power Up: " +
            powerUpData.powerUpName
        );
    }

    public PowerUpData GetPowerUpData()
    {
        return powerUpData;
    }
}