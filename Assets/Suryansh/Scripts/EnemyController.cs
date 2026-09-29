using UnityEngine;

public class EnemyController2D : MonoBehaviour
{
    public enum EnemyType { Normal, Ranged, Bomber }

    [Header("Enemy Settings")]
    public EnemyType enemyType;
    public float speed = 3f;
    public float attackRange = 5f; 
    public float explosionRadius = 1.5f; 
    
    private Transform player;
    private WaveManager waveManager;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void OnEnable()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
        
        if (waveManager == null) waveManager = FindFirstObjectByType<WaveManager>();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        
        FlipSprite();

        switch (enemyType)
        {
            case EnemyType.Normal:
                
                MoveTowardsPlayer();
                break;

            case EnemyType.Ranged:
                
                if (distanceToPlayer > attackRange)
                {
                    MoveTowardsPlayer();
                }
                else
                {
                    // yahan shooting script trigger kar sakta hai
                    // Debug.Log("Shooting Player!");
                }
                break;

            case EnemyType.Bomber:
                
                MoveTowardsPlayer();
                
                if (distanceToPlayer <= explosionRadius)
                {
                    Explode();
                }
                break;
        }
    }

    private void MoveTowardsPlayer()
    {
        
        transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
    }

    private void FlipSprite()
    {
        if (spriteRenderer != null)
        {
            
            if (player.position.x < transform.position.x)
                spriteRenderer.flipX = true;
            else if (player.position.x > transform.position.x)
                spriteRenderer.flipX = false;
        }
    }

    private void Explode()
    {
        Debug.Log("BOOM! Bomber Exploded!");
        
        DefeatEnemy();
    }

    public void DefeatEnemy()
    {
        if (waveManager != null) waveManager.EnemyDefeated();
        gameObject.SetActive(false);
    }
}