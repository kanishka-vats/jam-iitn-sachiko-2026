using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    [Header("Currency")]
    [SerializeField] private int startingDiamonds = 0;

    private int diamonds;

    public int Diamonds => diamonds;

    public event Action<int> OnDiamondsChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        diamonds = startingDiamonds;
    }

    public void AddDiamonds(int amount)
    {
        if (amount <= 0)
            return;

        diamonds += amount;

        Debug.Log(
            "Diamonds +" + amount +
            " | Total: " + diamonds
        );

        OnDiamondsChanged?.Invoke(diamonds);
    }

    public bool CanAfford(int cost)
    {
        return diamonds >= cost;
    }

    public bool SpendDiamonds(int amount)
    {
        if (amount <= 0)
            return false;

        if (diamonds < amount)
        {
            Debug.Log(
                "Not enough diamonds. " +
                "Required: " + amount +
                " | Current: " + diamonds
            );

            return false;
        }

        diamonds -= amount;

        Debug.Log(
            "Diamonds spent: " + amount +
            " | Remaining: " + diamonds
        );

        OnDiamondsChanged?.Invoke(diamonds);

        return true;
    }

    public void ResetDiamonds()
    {
        diamonds = startingDiamonds;

        OnDiamondsChanged?.Invoke(diamonds);
    }
}