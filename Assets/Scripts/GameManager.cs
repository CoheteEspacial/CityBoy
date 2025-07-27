using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Mission Settings")]
    [Tooltip("Random mission duration between min and max seconds")]
    public float missionDurationMin = 150f; // 2.5 min
    public float missionDurationMax = 240f; // 4 min

    [Tooltip("Number of missions completed before this one")]
    public int missionsCompleted = 0;

    [Tooltip("Percentage difficulty modifier from mission type (0.3 = 30%)")]
    public float missionTypeModifierPercent = 0.3f;

    [Tooltip("How early/late events should happen")]
    public float startParallaxTime = 5f;
    public float spawnerStartTime = 10f;
    public float spawnerStopBuffer = 15f;
    public float stopParallaxBuffer = 5f;

    [Header("Sun Control")]
    public Transform sunTransform;
    public float sunStartRotation = 50f;
    public float sunEndRotation = -50f;

    [Header("Color Filter")]
    public SpriteRenderer screenFilter;
    public Color sunriseColor = new Color(1f, 0.6f, 0.2f, 0.5f);
    public Color noonColor = new Color(1f, 1f, 1f, 0f); // No tint
    public Color sunsetColor = new Color(0.2f, 0.3f, 0.6f, 0.5f);

    [Header("External Script References")]
    public ParallaxScroller parallaxScript;
    public GameObject[] spawners;

    private float missionDuration;
    private float timer;
    private float intensity;
    private bool spawnersActive = false;




    public static GameManager Instance;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    void Start()
    {
        StartCoroutine(MissionRoutine());
    }

    IEnumerator MissionRoutine()
    {
        // Set mission duration
        missionDuration = Random.Range(missionDurationMin, missionDurationMax);

        // Calculate intensity
        float baseRandom = Random.Range(0.8f, 1.2f);
        float missionProgressBonus = missionsCompleted * 0.2f;
        float modifier = (baseRandom + missionProgressBonus);
        intensity = modifier + (modifier * missionTypeModifierPercent);

        Debug.Log($"Mission Intensity: {intensity}");

        // Time-based events
        timer = 0f;
        float halfTime = missionDuration / 2f;

        while (timer < missionDuration)
        {
            float t = timer / missionDuration;

            // Rotate sun
            if (sunTransform != null)
            {
                float sunRotZ = Mathf.Lerp(sunStartRotation, sunEndRotation, t);
                sunTransform.rotation = Quaternion.Euler(0f, 0f, sunRotZ);
            }

            // Update color filter
            if (screenFilter != null)
            {
                Color targetColor = Color.Lerp(sunriseColor, noonColor, Mathf.Min(1f, t * 2));
                if (t > 0.5f) targetColor = Color.Lerp(noonColor, sunsetColor, (t - 0.5f) * 2);
                screenFilter.color = targetColor;
            }

            // Start parallax
            if (timer >= startParallaxTime && parallaxScript != null)
            {
                parallaxScript.StartParallax();
            }

            // Start spawner
            if (timer >= spawnerStartTime && !spawnersActive)
            {
                foreach (var s in spawners)
                {
                    s.GetComponent<Spawner>()?.ActivateSpawner();
                }
                spawnersActive = true;
            }

            // Stop spawner
            if (timer >= missionDuration - spawnerStopBuffer && spawnersActive)
            {
                foreach (var s in spawners)
                {
                    s.GetComponent<Spawner>()?.DeactivateSpawner();
                }
                spawnersActive = false;
            }

            // Stop parallax
            if (timer >= missionDuration - stopParallaxBuffer && parallaxScript != null)
            {
                parallaxScript.StopParallax();
            }

            timer += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Mission Complete! Moving to next phase.");
        //StartNextPhase();
        FindFirstObjectByType<MissionEndUI>().Show(Player.Instance.turretTypes);
    }

    public void StartNextPhase()
    {
        Player.Instance.SaveState();
        // Later you’ll replace this with the real phase loader
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }


    public float GetMissionIntensity()
    {
        return intensity;
    }
}
