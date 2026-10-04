using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;

    // Other systems can listen to these later
    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    private void Awake()
    {
        currentHealth = maxHealth;

        // Sync UI immediately
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0)
            return;

        currentHealth -= damage;

        currentHealth = Mathf.Max(currentHealth, 0);

        AudioManager.Instance.PlaySFX(SFXType.PlayerHit);

        Debug.Log(
            "Player Health: " +
            currentHealth +
            "/" +
            maxHealth
        );

        OnHealthChanged?.Invoke(
            currentHealth,
            maxHealth
        );

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (currentHealth <= 0)
            return;

        currentHealth += amount;

        currentHealth = Mathf.Min(
            currentHealth,
            maxHealth
        );

        Debug.Log(
            "Player Health: " +
            currentHealth +
            "/" +
            maxHealth
        );

        OnHealthChanged?.Invoke(
            currentHealth,
            maxHealth
        );
    }

    private void Die()
    {
        Debug.Log("PLAYER DIED");

        AudioManager.Instance.PlaySFX(SFXType.PlayerDeath);

        OnDeath?.Invoke();
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public float GetHealthPercent()
    {
        return (float)currentHealth / maxHealth;
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    // Temporary testing method
    [ContextMenu("Test Damage")]
    private void TestDamage()
    {
        TakeDamage(10);
    }
}