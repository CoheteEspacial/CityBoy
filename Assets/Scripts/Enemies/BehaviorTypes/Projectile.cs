using UnityEngine;
using System.Collections.Generic;

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

    [Header("Multi-Hit Settings")]
    public bool multiHit = false;
    public float multiHitCooldown = 0.5f;

    private Dictionary<Collider2D, float> lastHitTime = new();

    public enum TargetType
    {
        Enemy,
        Player
    }

    public void SetDamage(float dmg) => damage = dmg;
    public void SetDirection(Vector2 dir) => direction = dir.normalized;
    public void SetSpeed(float spd) => speed = spd;

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsValidTarget(other)) return;

        if (multiHit)
        {
            if (lastHitTime.TryGetValue(other, out float lastTime))
            {
                if (Time.time < lastTime + multiHitCooldown)
                    return;
            }

            DealDamage(other);
            lastHitTime[other] = Time.time;
        }
        else
        {
            DealDamage(other);
            if (explodeOnHit) Explode();
            Destroy(gameObject);
        }
    }

    bool IsValidTarget(Collider2D other)
    {
        return (targetType == TargetType.Enemy && other.CompareTag("Enemy")) ||
               (targetType == TargetType.Player && other.CompareTag("Player"));
    }

    void DealDamage(Collider2D target)
    {
        switch (targetType)
        {
            case TargetType.Enemy:
                target.GetComponent<Enemy>()?.TakeDamage(damage);
                break;
            case TargetType.Player:
                target.GetComponent<Player>()?.TakeDamage(damage);
                break;
        }

        if (explodeOnHit) Explode();
    }

    void Explode()
    {
        if (explosionPrefab)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var col in hits)
        {
            if (IsValidTarget(col))
                DealDamage(col);
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
