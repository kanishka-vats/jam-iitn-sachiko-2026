using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float bulletSpeed = 15f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 5f;
    
    private Vector2 direction = Vector2.right;
    private Rigidbody2D rb;
    private float spawnTime;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spawnTime = Time.time;
        
        if (rb != null)
        {
            rb.linearVelocity = direction * bulletSpeed;
        }
    }

    private void Update()
    {
        if (Time.time - spawnTime > lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
    }
    
    public void SetSpeed(float speed)
    {
        bulletSpeed = speed;
        if (rb != null)
        {
            rb.linearVelocity = direction * bulletSpeed;
        }
    }
    
    public void SetDamage(int damageAmount)
    {
        damage = damageAmount;
    }
    
    public int GetDamage()
    {
        return damage;
    }

    // private void OnTriggerEnter2D(Collider2D collision)
    // {
    //     if (collision.CompareTag("Enemy"))
    //     {
    //         Enemy enemy = collision.GetComponent<Enemy>();
    //         if (enemy != null)
    //         {
    //             enemy.TakeDamage(damage);
    //         }
            
    //         Destroy(gameObject);
    //     }
    // }
}
