using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public float damage;
    private Vector2 direction;

    public TargetType targetType = TargetType.Enemy;

    [Header("Explosion Settings")]
    public bool explodeOnHit = false;
    public float explosionRadius = 1.5f;
    [SerializeField] private GameObject explosionPrefab;

    public enum TargetType
    {
        Enemy,
        Player
    }
    public void SetDamage(float dmg)
    {
        damage = dmg;
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }
    public void SetSpeed(float spd)
    {
        speed = spd;
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        switch (targetType)
        {
            case TargetType.Enemy:
                if (other.CompareTag("Enemy"))
                {
                    other.GetComponent<Enemy>()?.TakeDamage(damage);
                    Destroy(gameObject); // Destroy the projectile after hitting the target
                }
                break;

            case TargetType.Player:
                if (other.CompareTag("Player"))
                {
                    other.GetComponent<Player>()?.TakeDamage(damage);
                    Destroy(gameObject); // Destroy the projectile after hitting the target
                }
                break;
        }

        if (explodeOnHit)
        {
            Explode();
        }

    }

    void Explode()
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var col in hits)
        {
            switch (targetType)
            {
                case TargetType.Enemy:
                    if (col.CompareTag("Enemy"))
                        col.GetComponent<Enemy>()?.TakeDamage(damage);
                    break;

                case TargetType.Player:
                    if (col.CompareTag("Player"))
                        col.GetComponent<Player>()?.TakeDamage(damage);
                    break;
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (explodeOnHit)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
#endif
}
