using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyData data;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private float volume = 1f;
    

    [HideInInspector] public float currentHealth;
    private Transform player;
    public bool diesOnContact = false; // If true, the enemy dies when colliding with the player

    private void Awake()
    {
        currentHealth = data.health;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    public Transform GetPlayer()
    {
        return player;
    }

    public void TakeDamage(float amount)
    {
        Debug.Log($"Enemy Health: {currentHealth}");
        SoundFXManager.Instance.PlaySoundFXClip(deathSound, transform, volume);
        currentHealth -= amount;
        if (currentHealth <= 0) Destroy(GameObject);
        

    }

    public bool IsInRange(float distance)
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.position) <= distance;
    }

    public float GetDirectionToPlayer()
    {
        if (player == null) return 0;
        return Mathf.Sign(player.position.x - transform.position.x);
    }
   
}
