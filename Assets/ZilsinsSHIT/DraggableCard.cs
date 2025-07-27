using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic; // Added this namespace for List<>

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    public CardDisplaySystem cardDisplay;
    public Image cardImage;
    public CanvasGroup canvasGroup;

    [Header("Drag Settings")]
    public float returnDuration = 0.2f;
    public float dragAlpha = 0.8f;

    [HideInInspector] public ConveyorBeltSystem conveyorSystem;
    [HideInInspector] public CardData cardData;
    [HideInInspector] public CardPosition currentPosition;
    [HideInInspector] public Vector3 targetPosition;
    [HideInInspector] public bool isDragging;
    [SerializeField] private AudioClip castSound;
    [SerializeField] private float volume = 1f;

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Transform originalParent;
    private bool isReturning;
    private GameObject currentTarget;
    private Camera mainCamera;
    private Mouse currentMouse;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        mainCamera = Camera.main;
        currentMouse = Mouse.current;
    }

    void Start()
    {
        originalParent = transform.parent;

        if (cardData != null && cardDisplay != null)
        {
            cardDisplay.InitializeCard(cardData);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isReturning) return;

        isDragging = true;
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;

        // Move to top of hierarchy
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        // Visual feedback
        canvasGroup.alpha = dragAlpha;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || isReturning) return;

        // Follow mouse position
        Vector2 mousePosition = currentMouse.position.ReadValue();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            mousePosition,
            null,
            out Vector2 localPoint
        );
        rectTransform.localPosition = localPoint;

        // Highlight valid targets
        CheckForValidTarget(mousePosition);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging || isReturning) return;

        isDragging = false;
        canvasGroup.alpha = 1f;
        cardImage.color = Color.white;

        Vector2 mousePosition = currentMouse.position.ReadValue();

        // Check destroy zone first
        DestroyZone destroyZone = FindObjectOfType<DestroyZone>();
        if (destroyZone != null && destroyZone.IsPointInside(mousePosition))
        {
            if (conveyorSystem.CanDestroyCard())
            {
                conveyorSystem.DestroyCardForEnergy(this);
                return;
            }
        }

        // Check for valid target
        GameObject target = FindTargetUnderMouse(mousePosition);
        if (target != null && IsValidTarget(target))
        {
            if (conveyorSystem.TrySpendEnergy(cardData.energyCost))
            {
                ApplyCardEffect(target);
                conveyorSystem.RemoveCard(this);
            }
            else
            {
                StartCoroutine(ReturnToConveyor());
            }
        }
        else
        {
            StartCoroutine(ReturnToConveyor());
        }
    }

    private GameObject FindTargetUnderMouse(Vector2 mousePosition)
    {
        // Check UI elements first
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject == gameObject) continue;
            if (IsValidTarget(result.gameObject)) return result.gameObject;
        }

        // Check game objects with colliders
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        RaycastHit2D[] physicsHits = Physics2D.GetRayIntersectionAll(ray);

        foreach (RaycastHit2D hit in physicsHits)
        {
            if (hit.collider.gameObject == gameObject) continue;
            if (IsValidTarget(hit.collider.gameObject)) return hit.collider.gameObject;
        }

        return null;
    }

    private void CheckForValidTarget(Vector2 mousePosition)
    {
        GameObject target = FindTargetUnderMouse(mousePosition);
        cardImage.color = target != null ? Color.green : Color.white;
    }

    private bool IsValidTarget(GameObject target)
    {
        if (target == null || cardData == null) return false;

        switch (cardData.cardType)
        {
            case CardType.TurretBuff:
                return target.GetComponent<TurretScript>() != null;
            case CardType.CityBuff:
                //return target.GetComponent<CityManager>() != null;
            case CardType.EnemyEffect:
                //return target.GetComponent<Enemy>() != null;
            default:
                return false;
        }
    }

    private IEnumerator ReturnToConveyor()
    {
        isReturning = true;
        canvasGroup.blocksRaycasts = false;

        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < returnDuration)
        {
            transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                elapsed / returnDuration
            );
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        transform.SetParent(originalParent);
        canvasGroup.blocksRaycasts = true;
        isReturning = false;
    }

    private void ApplyCardEffect(GameObject target)
    {
        if (target == null) return;

        switch (cardData.cardType)
        {
            case CardType.TurretBuff:
                var turret = target.GetComponent<TurretScript>();
                if (turret != null)
                {
                    SoundFXManager.Instance.PlaySoundFXClip(castSound, transform, volume);
                    turret.ApplyBuff(
                        cardData.damageBuffPercent,
                        cardData.rangeBuffPercent,
                        cardData.fireRateBuffPercent,
                        cardData.buffDuration
                    );
                }
                break;

            case CardType.CityBuff:
                //var city = target.GetComponent<CityManager>();
                //if (city != null)
                //{
                //    city.ApplyBuff(
                //        cardData.damageBuffPercent,
                //        cardData.rangeBuffPercent,
                //        cardData.fireRateBuffPercent,
                //        cardData.buffDuration
                //    );
                //}
                break;

            case CardType.EnemyEffect:
                //var enemy = target.GetComponent<Enemy>();
                //if (enemy != null)
                //{
                //    enemy.ApplyEffect(
                //        cardData.damageBuffPercent,
                //        cardData.rangeBuffPercent,
                //        cardData.fireRateBuffPercent,
                //        cardData.buffDuration
                //    );
                //}
                break;
        }

        StartCoroutine(DestroyCard());
    }

    private IEnumerator DestroyCard()
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}