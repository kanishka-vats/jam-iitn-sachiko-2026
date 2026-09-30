using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float bulletSpeed = 15f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 5f;

    private Vector2 direction = Vector2.right;
    private float lifetimeTimer = 0f;

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
    }

    public void SetSpeed(float speed)
    {
        bulletSpeed = speed;
    }

    public void SetDamage(int damageAmount)
    {
        damage = damageAmount;
    }

    public int GetDamage()
    {
        return damage;
    }

    private void Update()
    {
        transform.position +=
            (Vector3)(direction * bulletSpeed * Time.deltaTime);

        lifetimeTimer += Time.deltaTime;

        if (lifetimeTimer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyBase enemy = collision.GetComponent<EnemyBase>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}