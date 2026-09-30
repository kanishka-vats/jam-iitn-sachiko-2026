using UnityEngine;

public class ArcherProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 6f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float lifetime = 5f;

    private Vector2 direction;

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
    }

    private void Update()
    {
        transform.Translate(
            direction * speed * Time.deltaTime,
            Space.World
        );

        lifetime -= Time.deltaTime;

        if (lifetime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        PlayerHealth playerHealth =
            collision.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}