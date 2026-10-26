
using UnityEngine;

public class BomberEnemy : EnemyBase
{
    [Header("Bomber Settings")]
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private int explosionDamage = 30;

    [Header("Explosion Effect")]
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private Vector3 explosionEffectOffset = Vector3.zero;

    [Header("Fire Trail")]
    [SerializeField] private GameObject fireTrailPrefab;
    [SerializeField] private float trailInterval = 0.2f;
    [SerializeField] private float trailDistanceBehind = 0.5f;
    [SerializeField] private Vector3 trailOffset = Vector3.zero;

    private Rigidbody2D rb;
    private float trailTimer;
    private Vector2 lastPosition;
    private bool hasExploded;

    protected override void Start()
    {
        base.Start();

        rb = GetComponent<Rigidbody2D>();
        lastPosition = rb != null ? rb.position : (Vector2)transform.position;
    }

    private void FixedUpdate()
    {
        if (player == null || IsMovementLocked || rb == null)
            return;

        Vector2 direction = ((Vector2)player.position - rb.position).normalized;

        rb.MovePosition(
            rb.position + direction * moveSpeed * Time.fixedDeltaTime
        );

        Vector2 actualMovement = rb.position - lastPosition;

        if (actualMovement.sqrMagnitude > 0.0001f)
        {
            trailTimer -= Time.fixedDeltaTime;

            if (trailTimer <= 0f)
            {
                SpawnFireTrail(actualMovement.normalized);
                trailTimer = trailInterval;
            }
        }

        lastPosition = rb.position;
    }

    private void SpawnFireTrail(Vector2 movementDirection)
    {
        if (fireTrailPrefab == null)
            return;

        Vector2 spawnPosition =
            rb.position - movementDirection * trailDistanceBehind
            + (Vector2)trailOffset;

        ParticleEffects.PlayOnce(
            fireTrailPrefab,
            spawnPosition,
            Quaternion.identity
        );
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            Explode();
    }

    private void Explode()
    {
        if (hasExploded)
            return;

        hasExploded = true;

        if (explosionEffectPrefab != null)
        {
            ParticleEffects.PlayOnce(
                explosionEffectPrefab,
                transform.position + explosionEffectOffset,
                Quaternion.identity
            );
        }

        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(
            transform.position,
            explosionRadius
        );

        foreach (Collider2D hitObject in hitObjects)
        {
            if (!hitObject.CompareTag("Player"))
                continue;

            PlayerHealth playerHealth = hitObject.GetComponent<PlayerHealth>();

            if (playerHealth != null)
                playerHealth.TakeDamage(explosionDamage);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
