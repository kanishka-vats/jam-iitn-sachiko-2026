using UnityEngine;

public class BomberEnemy : EnemyBase
{
    [Header("Bomber Settings")]
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private int explosionDamage = 30;

    private Rigidbody2D rb;

    protected override void Start()
    {
        base.Start();

        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (player == null)
            return;

        Vector2 direction =
            (player.position - transform.position).normalized;

        rb.MovePosition(
            rb.position +
            direction * moveSpeed * Time.fixedDeltaTime
        );
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(
            transform.position,
            explosionRadius
        );

        foreach (Collider2D hitObject in hitObjects)
        {
            if (hitObject.CompareTag("Player"))
            {
                PlayerHealth playerHealth =
                    hitObject.GetComponent<PlayerHealth>();

                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(explosionDamage);
                }
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            explosionRadius
        );
    }
}