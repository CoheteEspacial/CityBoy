using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    public Image cardImage;
    public Text energyCostText;
    public Text cardNameText;
    public Image cardTypeIcon;
    public Sprite turretIcon;
    public Sprite cityIcon;
    public Sprite enemyIcon;

    [HideInInspector] public ConveyorBeltSystem conveyorSystem;
    [HideInInspector] public CardData cardData;
    [HideInInspector] public CardPosition currentPosition;
    [HideInInspector] public Vector3 targetPosition;
    [HideInInspector] public bool isDragging;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;
    private bool isReturning;
    private GameObject currentTarget;
    private Camera mainCamera;
    private Mouse currentMouse;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        mainCamera = Camera.main;
        currentMouse = Mouse.current;
    }

    void Start()
    {
        originalParent = transform.parent;

        if (cardData != null)
        {
            //energyCostText.text = cardData.energyCost.ToString();
            //cardNameText.text = cardData.cardName;

            // Set card type icon
            switch (cardData.cardType)
            {
                case CardType.TurretBuff:
                    if (turretIcon != null) cardTypeIcon.sprite = turretIcon;
                    cardTypeIcon.color = Color.red;
                    break;
                case CardType.CityBuff:
                    if (cityIcon != null) cardTypeIcon.sprite = cityIcon;
                    cardTypeIcon.color = Color.blue;
                    break;
                case CardType.EnemyEffect:
                    if (enemyIcon != null) cardTypeIcon.sprite = enemyIcon;
                    cardTypeIcon.color = Color.green;
                    break;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isReturning) return;

        isDragging = true;
        originalPosition = rectTransform.anchoredPosition;

        // Save current parent
        originalParent = transform.parent;

        // Move to top of hierarchy
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        canvasGroup.alpha = 0.8f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || isReturning) return;

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

        // Reset visual feedback
        cardImage.color = Color.white;

        // Check if dropped on destroy zone
        Vector2 mousePosition = currentMouse.position.ReadValue();
        DestroyZone destroyZone = FindObjectOfType<DestroyZone>();
        if (destroyZone != null && destroyZone.IsPointInside(mousePosition))
        {
            if (conveyorSystem.CanDestroyCard())
            {
                conveyorSystem.DestroyCardForEnergy(this);
                return;
            }
        }

        // Find current target under mouse
        GameObject target = FindTargetUnderMouse(mousePosition);

        // Apply card effect if we have a valid target
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

        // Check for valid targets in UI
        foreach (RaycastResult result in results)
        {
            if (result.gameObject == gameObject) continue;

            if (IsValidTarget(result.gameObject))
            {
                return result.gameObject;
            }
        }

        // Check for game objects with colliders
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        RaycastHit2D[] physicsHits = Physics2D.GetRayIntersectionAll(ray);

        foreach (RaycastHit2D hit in physicsHits)
        {
            if (hit.collider.gameObject == gameObject) continue;

            if (IsValidTarget(hit.collider.gameObject))
            {
                return hit.collider.gameObject;
            }
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
        // Check if card type matches target type
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

        // Use conveyor system's return speed
        float returnSpeed = conveyorSystem.cardReturnSpeed;

        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                returnSpeed * Time.deltaTime
            );
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
                TurretScript turret = target.GetComponent<TurretScript>();
                if (turret != null)
                {
                    turret.ApplyBuff(
                        cardData.damageBuffPercent,
                        cardData.rangeBuffPercent,
                        cardData.fireRateBuffPercent,
                        cardData.buffDuration
                    );
                }
                break;

            case CardType.CityBuff:
                //CityManager city = target.GetComponent<CityManager>();
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
                //Enemy enemy = target.GetComponent<Enemy>();
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