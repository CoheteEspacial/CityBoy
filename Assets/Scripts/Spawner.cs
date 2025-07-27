using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyTierData
{
    public GameObject prefab;
    public int tierLevel = 1; // 1 = weak, 2 = mid, 3 = strong
    public float baseWeight = 1f;
}

public class Spawner : MonoBehaviour
{
    [Header("Spawn Timing")]
    public float spawnRateMin = 0.5f;
    public float spawnRateMax = 3f;
    public int maxConcurrentEnemies = 10;

    [Header("Spawn Area")]
    public Vector2 ySpawnRange = new Vector2(-3f, 3f);
    public Transform spawnParent;

    [Header("Enemy Tiers")]
    public List<EnemyTierData> enemyTiers = new();

    private GameManager gameManager;
    private float missionDuration;
    private float elapsedTime = 0f;
    private float intensity = 1f;
    private bool isSpawning = false;
    private List<GameObject> spawnedEnemies = new();

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            intensity = gameManager.GetMissionIntensity();
            missionDuration = Random.Range(gameManager.missionDurationMin, gameManager.missionDurationMax);
        }
    }

    public void ActivateSpawner()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            StartCoroutine(SpawnerRoutine());
        }
    }

    public void DeactivateSpawner()
    {
        isSpawning = false;
    }

    IEnumerator SpawnerRoutine()
    {
        while (isSpawning)
        {
            elapsedTime += Time.deltaTime;
            if (spawnedEnemies.RemoveAll(e => e == null) >= 0 && spawnedEnemies.Count >= maxConcurrentEnemies)
            {
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            float t = Mathf.Clamp01(elapsedTime / missionDuration);
            float baseRate = Mathf.Lerp(spawnRateMax, spawnRateMin, Mathf.Sin(t * Mathf.PI));
            float randomModifier = Random.Range(0.85f, 1.15f);
            float spawnDelay = baseRate * randomModifier;

            GameObject enemyToSpawn = PickEnemyBasedOnTier(t);
            if (enemyToSpawn != null)
            {
                float y = Random.Range(ySpawnRange.x, ySpawnRange.y);
                Vector3 spawnPos = new Vector3(transform.position.x, y, 0f);
                GameObject enemy = Instantiate(enemyToSpawn, spawnPos, Quaternion.identity, transform);
                spawnedEnemies.Add(enemy);
            }

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    GameObject PickEnemyBasedOnTier(float progress)
    {
        float totalWeight = 0f;
        List<float> tierWeights = new();

        foreach (var tier in enemyTiers)
        {
            float scaledWeight = tier.baseWeight * intensity * (1 + tier.tierLevel * progress);
            tierWeights.Add(scaledWeight);
            totalWeight += scaledWeight;
        }

        float rand = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < enemyTiers.Count; i++)
        {
            cumulative += tierWeights[i];
            if (rand <= cumulative)
            {
                return enemyTiers[i].prefab;
            }
        }

        return null;
    }
}