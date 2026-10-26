
using UnityEngine;

public class ArcherEnemy : EnemyBase
{
    [Header("Archer Settings")]
    [SerializeField] private float preferredDistance = 6f;
    [SerializeField] private float minimumDistance = 4f;
    [SerializeField] private float attackRange = 8f;
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Purple Aura")]
    [SerializeField] private GameObject auraPrefab;
    [SerializeField] private Vector3 auraLocalOffset = Vector3.zero;
    [SerializeField] private int auraSortingOrder = -1;

    private Rigidbody2D rb;
    private float fireTimer;
    private GameObject activeAura;

    protected override void Start()
    {
        base.Start();

        rb = GetComponent<Rigidbody2D>();

        if (auraPrefab != null)
        {
            activeAura = ParticleEffects.Attach(
                auraPrefab,
                transform,
                auraLocalOffset
            );

            if (activeAura != null)
            {
                activeAura.transform.localPosition = auraLocalOffset;

                SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

                foreach (ParticleSystemRenderer particleRenderer in
                         activeAura.GetComponentsInChildren<ParticleSystemRenderer>(true))
                {
                    if (spriteRenderer != null)
                        particleRenderer.sortingLayerID = spriteRenderer.sortingLayerID;

                    particleRenderer.sortingOrder = auraSortingOrder;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (player == null || IsMovementLocked || rb == null)
            return;

        float distance = Vector2.Distance(transform.position, player.position);
        Vector2 direction = ((Vector2)player.position - rb.position).normalized;

        if (distance > preferredDistance)
        {
            rb.MovePosition(
                rb.position + direction * moveSpeed * Time.fixedDeltaTime
            );
        }
        else if (distance < minimumDistance)
        {
            rb.MovePosition(
                rb.position - direction * moveSpeed * Time.fixedDeltaTime
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
        Debug.Log("Archer Shoot() called");

        if (projectilePrefab == null)
        {
            Debug.LogError("Archer projectilePrefab is not assigned!", this);
            return;
        }

        if (player == null)
        {
            Debug.LogError("Archer cannot find the Player!", this);
            return;
        }

        Vector2 direction =
            ((Vector2)player.position - (Vector2)transform.position).normalized;

        GameObject projectile = Instantiate(
            projectilePrefab,
            transform.position,
            Quaternion.identity
        );

        Debug.Log("Fire projectile spawned: " + projectile.name);

        ArcherFireProjectile fireProjectile =
            projectile.GetComponent<ArcherFireProjectile>();

        if (fireProjectile != null)
        {
            fireProjectile.SetDirection(direction);
        }
        else
        {
            Debug.LogError(
                "ArcherFireProjectile script is missing from the projectile prefab!",
                projectile
            );
        }
    }

    protected override void Die()
    {
        if (activeAura != null)
        {
            ParticleEffects.StopAndDestroy(activeAura);
            activeAura = null;
        }

        base.Die();
    }

    private void OnDestroy()
    {
        if (activeAura != null)
        {
            ParticleEffects.StopAndDestroy(activeAura);
            activeAura = null;
        }
    }
}
