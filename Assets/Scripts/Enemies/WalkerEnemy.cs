using UnityEngine;

public class WalkerEnemy : EnemyBase
{
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
            PlayerHealth playerHealth =
                collision.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage((int)damage);
            }
        }
    }
}