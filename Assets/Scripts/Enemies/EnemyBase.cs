
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHealth = 100f;
    public float moveSpeed = 2f;
    public float damage = 10f;

    protected float currentHealth;
    protected Transform player;

    private float movementLockedUntil;

    protected bool IsMovementLocked => Time.time < movementLockedUntil;

    protected virtual void Start()
    {
        currentHealth = maxHealth;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;
    }

    public void HoldMovement(float duration)
    {
        movementLockedUntil = Mathf.Max(
            movementLockedUntil,
            Time.time + duration
        );
    }

    public virtual void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth <= 0f)
            Die();
    }

    protected virtual void Die()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.AddDiamonds(1);

        Destroy(gameObject);
    }
}
