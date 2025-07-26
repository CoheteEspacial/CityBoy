using UnityEngine;
using UnityEngine.UI;

public class EnergyDisplay : MonoBehaviour
{
    public Slider energySlider;
    //public Text energyText;
    //public Image cooldownIndicator;
    //public Text cooldownText;

    private ConveyorBeltSystem conveyorSystem;

    void Start()
    {
        conveyorSystem = FindObjectOfType<ConveyorBeltSystem>();
        if (conveyorSystem != null)
        {
            energySlider.maxValue = conveyorSystem.maxEnergy;
        }
    }

    void Update()
    {
        if (conveyorSystem != null)
        {
            // Update energy display
            energySlider.value = conveyorSystem.currentEnergy;
            //energyText.text = $"{conveyorSystem.currentEnergy:F0}/{conveyorSystem.maxEnergy}";

            // Update destroy cooldown indicator
            if (conveyorSystem.IsDestroyOnCooldown())
            {
                float progress = conveyorSystem.GetCooldownProgress();
                //cooldownIndicator.fillAmount = 1 - progress;

                // Show cooldown time remaining
                float timeLeft = conveyorSystem.destroyCooldown * (1 - progress);
                //cooldownText.text = $"{timeLeft:F1}s";
            }
            else
            {
                //cooldownIndicator.fillAmount = 0;
                //cooldownText.text = "Ready";
            }
        }
    }
}