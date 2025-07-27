using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class CardDisplaySystem : MonoBehaviour
{
    [Header("References")]
    public Text energyCostText;
    public Text cardNameText;
    public Text durationText;
    public Image damageIcon;
    public Text damageText;
    public Image rangeIcon;
    public Text rangeText;
    public Image fireRateIcon;
    public Text fireRateText;

    [Header("Visual Settings")]
    public Color highlightColor = Color.yellow;
    public Color normalColor = Color.white;
    public Sprite damageSprite;
    public Sprite rangeSprite;
    public Sprite fireRateSprite;

    public void InitializeCard(CardData cardData)
    {
        // Set basic info
        energyCostText.text = cardData.energyCost.ToString();
        //cardNameText.text = cardData.cardName;
        durationText.text = $"{cardData.buffDuration}s";

        // Set buff values
        damageText.text = cardData.damageBuffPercent > 0 ? $"+{cardData.damageBuffPercent}%" : "";
        rangeText.text = cardData.rangeBuffPercent > 0 ? $"+{cardData.rangeBuffPercent}%" : "";
        fireRateText.text = cardData.fireRateBuffPercent > 0 ? $"+{cardData.fireRateBuffPercent}%" : "";

        // Show only non-zero buffs
        damageIcon.gameObject.SetActive(cardData.damageBuffPercent > 0);
        rangeIcon.gameObject.SetActive(cardData.rangeBuffPercent > 0);
        fireRateIcon.gameObject.SetActive(cardData.fireRateBuffPercent > 0);

        // Set icons
        //damageIcon.sprite = damageSprite;
        //rangeIcon.sprite = rangeSprite;
        //fireRateIcon.sprite = fireRateSprite;

        // Highlight the highest buff
        HighlightHighestBuff(cardData);
    }

    private void HighlightHighestBuff(CardData cardData)
    {
        // Reset all colors first
        //damageIcon.color = normalColor;
        //rangeIcon.color = normalColor;
        //fireRateIcon.color = normalColor;
        damageText.color = normalColor;
        rangeText.color = normalColor;
        fireRateText.color = normalColor;

        // Find the highest buff value
        float maxValue = Mathf.Max(
            cardData.damageBuffPercent,
            cardData.rangeBuffPercent,
            cardData.fireRateBuffPercent
        );

        // Highlight all buffs that match the max value
        if (cardData.damageBuffPercent == maxValue && maxValue > 0)
        {
            //damageIcon.color = highlightColor;
            damageText.color = highlightColor;
        }

        if (cardData.rangeBuffPercent == maxValue && maxValue > 0)
        {
            //rangeIcon.color = highlightColor;
            rangeText.color = highlightColor;
        }

        if (cardData.fireRateBuffPercent == maxValue && maxValue > 0)
        {
            //fireRateIcon.color = highlightColor;
            fireRateText.color = highlightColor;
        }
    }
}