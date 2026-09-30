using UnityEngine;

public class ArcherEnemy : EnemyBase
{
    [Header("Archer Settings")]
    [SerializeField] private float preferredDistance = 6f;
    [SerializeField] private float minimumDistance = 4f;
    [SerializeField] private float attackRange = 8f;
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private GameObject projectilePrefab;

    private Rigidbody2D rb;
    private float fireTimer;

    protected override void Start()
    {
        base.Start();

        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (player == null)
            return;

        float distance =
            Vector2.Distance(transform.position, player.position);

        Vector2 direction =
            (player.position - transform.position).normalized;

        if (distance > preferredDistance)
        {
            rb.MovePosition(
                rb.position +
                direction * moveSpeed * Time.fixedDeltaTime
            );
        }
        else if (distance < minimumDistance)
        {
            rb.MovePosition(
                rb.position -
                direction * moveSpeed * Time.fixedDeltaTime
            );
        }

        if (distance <= attackRange)
        {
            fireTimer -= Time.fixedDeltaTime;

            if (fireTimer <= 0f)
            {
                Shoot();
                fireTimer = fireRate;
            }
        }
    }

    private void Shoot()
    {
        if (projectilePrefab == null || player == null)
            return;

        Vector2 direction =
            (player.position - transform.position).normalized;

        GameObject projectile = Instantiate(
            projectilePrefab,
            transform.position,
            Quaternion.identity
        );

        ArcherProjectile projectileScript =
            projectile.GetComponent<ArcherProjectile>();

        if (projectileScript != null)
        {
            projectileScript.SetDirection(direction);
        }
    }

}