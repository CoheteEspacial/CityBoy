using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Card Settings")]
    public CardType cardType;
    public float returnSpeed = 10f;

    [Header("Turret Buff Settings")]
    [Range(0, 100)] public float damageBuffPercent = 10f;
    [Range(0, 100)] public float rangeBuffPercent = 10f;
    [Range(0, 100)] public float fireRateBuffPercent = 10f;
    public float buffDuration = 5f;

    [Header("Visual Feedback")]
    public Color validTargetColor = Color.green;
    public Color invalidTargetColor = Color.red;
    public float dragAlpha = 0.8f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;
    private bool isDragging;
    private bool isReturning;
    private GameObject currentTarget;
    private Image cardImage;
    private int originalSiblingIndex;
    private bool isUsed;
    private Camera mainCamera;
    private Mouse currentMouse;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        cardImage = GetComponent<Image>();
        originalParent = transform.parent;
        mainCamera = Camera.main;
        currentMouse = Mouse.current;
    }

    void Start()
    {
        originalPosition = rectTransform.anchoredPosition;
        originalSiblingIndex = transform.GetSiblingIndex();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isReturning || isUsed) return;

        originalPosition = rectTransform.anchoredPosition;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        isDragging = true;
        canvasGroup.alpha = dragAlpha;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || isReturning || isUsed) return;

        Vector2 mousePosition = currentMouse.position.ReadValue();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            mousePosition,
            null,
            out Vector2 localPoint
        );
        rectTransform.localPosition = localPoint;

        CheckForValidTarget(mousePosition);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging || isReturning || isUsed) return;

        isDragging = false;
        canvasGroup.alpha = 1f;
        cardImage.color = Color.white;

        if (currentTarget != null)
        {
            ApplyCardEffect();
            HandleCardUsage();
        }
        else
        {
            StartCoroutine(ReturnToOriginalPosition());
        }
    }

    private void CheckForValidTarget(Vector2 mousePosition)
    {
        currentTarget = null;
        cardImage.color = Color.white;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        RaycastHit2D[] physicsHits = Physics2D.GetRayIntersectionAll(ray);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject == gameObject) continue;

            if (IsValidTarget(result.gameObject))
            {
                currentTarget = result.gameObject;
                cardImage.color = validTargetColor;
                return;
            }
        }

        foreach (RaycastHit2D hit in physicsHits)
        {
            if (hit.collider.gameObject == gameObject) continue;

            if (IsValidTarget(hit.collider.gameObject))
            {
                currentTarget = hit.collider.gameObject;
                cardImage.color = validTargetColor;
                return;
            }
        }
    }

    private bool IsValidTarget(GameObject target)
    {
        // Allow cards to be applied to turrets
        return target.GetComponent<TurretScript>() != null;
    }

    private IEnumerator ReturnToOriginalPosition()
    {
        if (isReturning) yield break;

        isReturning = true;
        canvasGroup.blocksRaycasts = false;

        Vector2 startPosition = rectTransform.anchoredPosition;
        float distance = Vector2.Distance(startPosition, originalPosition);
        float duration = Mathf.Clamp(distance / returnSpeed, 0.1f, 1f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (isUsed) yield break;

            rectTransform.anchoredPosition = Vector2.Lerp(
                startPosition,
                originalPosition,
                elapsed / duration
            );
            elapsed += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = originalPosition;
        transform.SetParent(originalParent);
        transform.SetSiblingIndex(originalSiblingIndex);
        canvasGroup.blocksRaycasts = true;
        isReturning = false;
    }

    private void ApplyCardEffect()
    {
        if (currentTarget == null || isUsed) return;

        TurretScript turret = currentTarget.GetComponent<TurretScript>();
        if (turret != null)
        {
            turret.ApplyBuff(
                damageBuffPercent,
                rangeBuffPercent,
                fireRateBuffPercent,
                buffDuration
            );
        }
    }

    private void HandleCardUsage()
    {
        if (isUsed) return;
        isUsed = true;
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

        transform.SetParent(originalParent);
        Destroy(gameObject);
    }
}

public enum CardType { TurretBuff, CityBuff, EnemyEffect }

// Sample classes for bullets and flames
public class Bullet : MonoBehaviour
{
    public float damage = 1f;
    // Other bullet logic...
}

public class Flame : MonoBehaviour
{
    public float damage = 1f;
    // Other flame logic...
}