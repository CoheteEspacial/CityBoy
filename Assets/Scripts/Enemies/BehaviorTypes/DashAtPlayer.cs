using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DashAtPlayer : MonoBehaviour
{
    private Enemy enemy;
    private Rigidbody2D rb;
    private float lastDashTime = -Mathf.Infinity;
    private Vector2 jumpStartY;
    private bool isReturning = false;

    [Header("Dash Settings")]
    public float dashTriggerDistance = 2f;
    public float dashForce = 12f;
    public float dashCooldown = 2f;

    [Header("Return Settings")]
    public float knockbackForce = 5f;
    public bool usesGravity = true; // If false, gravity is not used during return
    public float returnGravityScale = 2f;

    void Start()
    {
        enemy = GetComponent<Enemy>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isReturning) return; // Skip if mid-air return
        Transform player = enemy.GetPlayer();
        if (!player) return;
        float distance = Mathf.Abs(player.position.x - transform.position.x);
        if (distance < dashTriggerDistance && Time.time > lastDashTime + dashCooldown)
        {
            Vector2 dir = (enemy.GetPlayer().position - transform.position).normalized;
            rb.AddForce(dir * dashForce, ForceMode2D.Impulse);
            jumpStartY = transform.position;
            lastDashTime = Time.time;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        

            
        
        if (isReturning) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if (enemy.diesOnContact)
            {
                enemy.TakeDamage(enemy.currentHealth); // Kill the enemy on contact
            }
            else
            {
                enemy.TakeDamage(1);
            }

            collision.GetComponent<Player>()?.TakeDamage(enemy.data.damage); // Deal damage to the player

            Vector2 playerPos = enemy.GetPlayer().position;
            float direction = Mathf.Sign(transform.position.x - playerPos.x);

            // Knockback
            rb.linearVelocity = Vector2.zero; // Reset any current velocity
            rb.AddForce(new Vector2(direction * knockbackForce, knockbackForce), ForceMode2D.Impulse);

            // Start return process
            isReturning = true;
            if (usesGravity)
            {
                StartCoroutine(ReturnToGround());
            }
            
        }
    }

    private System.Collections.IEnumerator ReturnToGround()
    {
        rb.gravityScale = returnGravityScale;

        // Wait until we reach or fall below the original Y
        while (transform.position.y > jumpStartY.y)
        {
            yield return null;
        }

        // Snap to ground level
        Vector3 pos = transform.position;
        pos.y = jumpStartY.y;
        transform.position = pos;

        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;
        isReturning = false;
    }
}
