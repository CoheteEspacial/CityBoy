using UnityEngine;

public class ChasePlayer : MonoBehaviour
{
    private Enemy enemy;
    public float moveSpeed = 2f; // Speed at which the enemy moves towards the player
    public float stopDistance = 1.5f; // Distance at which the enemy stops chasing

    void Start()
    {
        enemy = GetComponent<Enemy>();
    }

    void Update()
    {
        Transform player = enemy.GetPlayer();
        if (!player) return;

        float distance = Mathf.Abs(player.position.x - transform.position.x);
        if (distance > stopDistance)
        {
            float dir = enemy.GetDirectionToPlayer();
            transform.position += new Vector3(dir * moveSpeed * Time.deltaTime, 0, 0);
        }
    }
}
