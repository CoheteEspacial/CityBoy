using UnityEngine;

public class Player : MonoBehaviour
{
    public float maxHealth = 100f; // Maximum health of the player
    private float currentHealth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    private void Awake()
    {
        currentHealth = maxHealth; // Initialize current health to maximum health

    }


    public void TakeDamage(float amount)
    {
        Debug.Log($"Took damage, current health: {currentHealth}");
        currentHealth -= amount;
        if (currentHealth <= 0) Death();
    }

    public void Death()
    {
        Debug.Log("Player has died.");
        // Handle player death logic here, such as restarting the level or showing a game over screen
        Destroy(gameObject);
    }
    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth; // Ensure health does not exceed maximum
        Debug.Log($"Healed, current health: {currentHealth}");
    }
}
