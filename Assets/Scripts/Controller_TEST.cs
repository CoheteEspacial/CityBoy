using UnityEngine;

public class Controller_TEST : MonoBehaviour
{
    public ParallaxScroller parallax; // Reference to your ParallaxController script

    private bool isParallaxRunning = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isParallaxRunning)
            {
                parallax.StopParallax();
            }
            else
            {
                parallax.StartParallax();
            }

            isParallaxRunning = !isParallaxRunning;
        }
    }
}
