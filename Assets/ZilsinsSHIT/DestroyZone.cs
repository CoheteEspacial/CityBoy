using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DestroyZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Appearance")]
    public Color activeColor = Color.red;
    public Color cooldownColor = Color.gray;

    private Image zoneImage;
    private bool isPointerOver;
    private ConveyorBeltSystem conveyorSystem;

    void Start()
    {
        zoneImage = GetComponent<Image>();
        conveyorSystem = FindObjectOfType<ConveyorBeltSystem>();
        UpdateAppearance();
    }

    void Update()
    {
        UpdateAppearance();
    }

    private void UpdateAppearance()
    {
        if (conveyorSystem != null)
        {
            zoneImage.color = conveyorSystem.CanDestroyCard() ? activeColor : cooldownColor;
        }
    }

    public bool IsPointInside(Vector2 screenPoint)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            GetComponent<RectTransform>(),
            screenPoint
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
    }
}