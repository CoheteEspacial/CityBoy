using System.Collections.Generic;
using UnityEngine;
using static TurretScript;

public class Player : MonoBehaviour
{
    public static Player Instance;

    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Turret Setup")]
    public List<TurretType> turretTypes = new();  // This stores selected turret types

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        currentHealth = maxHealth;
        LoadState(); // Load turrets/health on start
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"Took damage. Current Health: {currentHealth}");
        if (currentHealth <= 0) Death();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Healed. Current Health: {currentHealth}");
    }

    public void Death()
    {
        Debug.Log("Player has died.");
        Destroy(gameObject);
    }

    public void SaveState()
    {
        PlayerPrefs.SetFloat("Health", currentHealth);
        PlayerPrefs.SetInt("TurretCount", turretTypes.Count);
        for (int i = 0; i < turretTypes.Count; i++)
        {
            PlayerPrefs.SetInt($"Turret_{i}", (int)turretTypes[i]);
        }

        PlayerPrefs.Save();
        Debug.Log("Player state saved.");
    }

    public void LoadState()
    {
        currentHealth = PlayerPrefs.GetFloat("Health", maxHealth);
        int count = PlayerPrefs.GetInt("TurretCount", 0);
        turretTypes.Clear();
        for (int i = 0; i < count; i++)
        {
            turretTypes.Add((TurretType)PlayerPrefs.GetInt($"Turret_{i}"));
        }

        Debug.Log($"Loaded {turretTypes.Count} turrets and {currentHealth} health.");
    }
}
