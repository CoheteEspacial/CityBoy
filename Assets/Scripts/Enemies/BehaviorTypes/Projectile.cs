using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    private float damage;
    private Vector2 direction;

    public TargetType targetType = TargetType.Enemy;
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

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Deal damage to player here
            Debug.Log($"Hit Player for {damage} damage!");
            Destroy(gameObject);
        }

        // Optionally: destroy on hitting terrain
    }
}
