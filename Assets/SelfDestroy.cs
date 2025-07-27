using UnityEngine;

public class SelfDestroy : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        Destroy(gameObject, .1f); // Destroys the GameObject after 5 seconds
    }
}
