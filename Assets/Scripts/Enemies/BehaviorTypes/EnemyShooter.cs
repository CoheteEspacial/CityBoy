using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyShooter : MonoBehaviour
{
    public Transform shootPoint;                 // Where the bullet comes from
    public Transform turretPartToRotate;         // Part that rotates to aim at player
    public GameObject projectilePrefab;          // What to shoot
    public float shootCooldown = 1f;             // Delay between shots
    public float shootingDistance = 10f;         // Max range to start shooting
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private float volume = 1f;

    private Enemy enemy;
    private Transform player;
    private float lastShootTime = -Mathf.Infinity;

    void Start()
    {
        enemy = GetComponent<Enemy>();
        player = GameObject.FindWithTag("Player")?.transform;

        if (!player) Debug.LogWarning("Player not found! Make sure the player has the tag 'Player'.");
    }

    void Update()
    {
        if (!player) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= shootingDistance)
        {
            AimAtPlayer();

            if (Time.time >= lastShootTime + shootCooldown)
            {

                Shoot();
                lastShootTime = Time.time;
            }
        }
    }

    void AimAtPlayer()
    {
        if (!turretPartToRotate) return;

        Vector2 direction = (player.position - turretPartToRotate.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        turretPartToRotate.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Shoot()
    {
        SoundFXManager.Instance.PlaySoundFXClip(shootSound, transform, volume);
        if (!projectilePrefab || !shootPoint) return;

        GameObject bullet = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);

        Projectile proj = bullet.GetComponent<Projectile>();
        if (proj)
        {
            proj.SetDamage(enemy.data.damage);
            proj.SetDirection((player.position - shootPoint.position).normalized);
        }
    }
}
