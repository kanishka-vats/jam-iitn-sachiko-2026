using UnityEngine;
using TMPro;

public class CurrencyUI : MonoBehaviour
{
    [SerializeField] private TMP_Text diamondText;

    private void Start()
    {
        if (CurrencyManager.Instance != null)
        {
            UpdateDiamondText(
                CurrencyManager.Instance.Diamonds
            );

            CurrencyManager.Instance.OnDiamondsChanged += UpdateDiamondText;
        }
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnDiamondsChanged -= UpdateDiamondText;
        }
    }

    private void UpdateDiamondText(int amount)
    {
        if (diamondText != null)
        {
            diamondText.text = amount.ToString();
        }
    }
}