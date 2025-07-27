using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject enemyPrefab;
    public float spawnRateMin = 0.5f;  // spawn units per second (start)
    public float spawnRateMax = 2f;    // spawn units per second (max over time)
    public float timeToMaxRate = 60f;  // time in seconds to reach max spawn rate

    private bool spawning = false;
    private float elapsedTime = 0f;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (spawning)
        {
            // Calculate current spawn rate based on elapsed time
            float t = Mathf.Clamp01(elapsedTime / timeToMaxRate);
            float currentRate = Mathf.Lerp(spawnRateMin, spawnRateMax, t);

            // Spawn enemy
            Instantiate(enemyPrefab, transform.position, Quaternion.identity);

            // Wait for next spawn based on rate (units/second => delay = 1 / rate)
            float delay = 1f / currentRate;
            yield return new WaitForSeconds(delay);

            elapsedTime += delay;
        }
    }

    // Public function to stop spawning
    public void StartSpawning()
    {
        spawning = true;
    }

    public void StopSpawning()
    {
        spawning = false;
    }
}
