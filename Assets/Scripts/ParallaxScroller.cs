using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ParallaxLayer
{
    public Sprite sprite;
    public float scrollSpeed = 1f;
    public float yOffset = 0f;      // NEW: Allows setting vertical position
    public int sOrder = 0;          // Determines both Z position and sortingOrder

    [HideInInspector] public List<Transform> activeInstances = new();
    [HideInInspector] public float spriteWidth;
}

public class ParallaxScroller : MonoBehaviour
{
    public List<ParallaxLayer> layers = new();
    public Transform cameraTransform;
    public float smoothStartStopSpeed = 1f;

    private float scrollFactor = 0f;
    private float targetScrollFactor = 0f;

    private Camera mainCam;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        mainCam = Camera.main;

        foreach (var layer in layers)
        {
            // Calculate width of the sprite in world units
            GameObject temp = new GameObject("Temp");
            SpriteRenderer sr = temp.AddComponent<SpriteRenderer>();
            sr.sprite = layer.sprite;
            sr.sortingOrder = layer.sOrder;
            layer.spriteWidth = sr.bounds.size.x;
            Destroy(temp);

            // Create enough sprites to fill the screen + extra buffer
            float screenWidth = mainCam.orthographicSize * 2 * mainCam.aspect;
            int neededCount = Mathf.CeilToInt(screenWidth / layer.spriteWidth) + 2;

            for (int i = 0; i < neededCount; i++)
            {
                Vector3 pos = new Vector3(i * layer.spriteWidth, layer.yOffset, 0); // Use sOrder as -Z
                Transform instance = CreateSpriteInstance(layer, pos);
                layer.activeInstances.Add(instance);
            }
        }
    }

    void Update()
    {
        scrollFactor = Mathf.MoveTowards(scrollFactor, targetScrollFactor, Time.deltaTime * smoothStartStopSpeed);

        foreach (var layer in layers)
        {
            for (int i = layer.activeInstances.Count - 1; i >= 0; i--)
            {
                Transform t = layer.activeInstances[i];
                t.position += Vector3.left * layer.scrollSpeed * scrollFactor * Time.deltaTime;

                float camLeftEdge = cameraTransform.position.x - GetScreenHalfWidth();
                if (t.position.x + layer.spriteWidth < camLeftEdge)
                {
                    Transform last = layer.activeInstances[layer.activeInstances.Count - 1];
                    t.position = new Vector3(last.position.x + layer.spriteWidth, layer.yOffset, -layer.sOrder);

                    layer.activeInstances.RemoveAt(i);
                    layer.activeInstances.Add(t);
                }
            }
        }
    }

    private float GetScreenHalfWidth()
    {
        return mainCam.orthographicSize * mainCam.aspect;
    }

    private Transform CreateSpriteInstance(ParallaxLayer layer, Vector3 position)
    {
        GameObject go = new GameObject("Parallax Sprite");
        go.transform.SetParent(transform);
        go.transform.position = position;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = layer.sprite;
        sr.sortingOrder = layer.sOrder;  // Keep this for 2D rendering

        return go.transform;
    }

    // Public controls
    public void StartParallax()
    {
        targetScrollFactor = 1f;
    }

    public void StopParallax()
    {
        targetScrollFactor = 0f;
    }
}
