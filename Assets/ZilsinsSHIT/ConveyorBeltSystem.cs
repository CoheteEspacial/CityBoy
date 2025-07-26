using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ConveyorBeltSystem : MonoBehaviour
{
    [Header("Conveyor Settings")]
    public Transform startPosition;
    public Transform middlePosition;
    public Transform endPosition;
    public float conveyorSpeed = 1f;
    public float cardReturnSpeed = 2000f; // Faster return
    public int maxCards = 3;

    [Header("Card Settings")]
    public GameObject cardPrefab;
    public List<CardData> deck = new List<CardData>();
    public float cardSpawnInterval = 2f;

    [Header("Energy Settings")]
    public float energyRegenRate = 1f;
    public float maxEnergy = 100f;
    public float currentEnergy = 50f;
    public float destroyRefundPercent = 0.5f;
    public float destroyCooldown = 5f;

    private List<DraggableCard> cardsOnConveyor = new List<DraggableCard>();
    private float spawnTimer;
    private float _lastDestroyTime;
    private bool _isDestroyOnCooldown;

    // Card position tracking
    private Dictionary<DraggableCard, CardPosition> cardPositions = new Dictionary<DraggableCard, CardPosition>();
    private Dictionary<CardPosition, DraggableCard> positionOccupants = new Dictionary<CardPosition, DraggableCard>();
    private bool needsAdvancement = false;

    // NEW: Track which cards are currently on conveyor
    private HashSet<string> activeCardNames = new HashSet<string>();

    void Start()
    {
        spawnTimer = cardSpawnInterval;
        InitializePositionDictionaries();
    }

    void Update()
    {
        currentEnergy = Mathf.Min(maxEnergy, currentEnergy + energyRegenRate * Time.deltaTime);
        
        if (_isDestroyOnCooldown && Time.time - _lastDestroyTime > destroyCooldown)
        {
            _isDestroyOnCooldown = false;
        }
        
        if (cardsOnConveyor.Count < maxCards)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0)
            {
                TrySpawnCard();
                spawnTimer = cardSpawnInterval;
            }
        }
        
        MoveCardsOnConveyor();
        
        // Check if we need to advance cards
        if (needsAdvancement)
        {
            TryAdvanceCards();
            needsAdvancement = false;
        }
    }

    private void InitializePositionDictionaries()
    {
        positionOccupants[CardPosition.Start] = null;
        positionOccupants[CardPosition.Middle] = null;
        positionOccupants[CardPosition.End] = null;
    }

    private void TrySpawnCard()
    {
        if (deck.Count == 0 || !IsPositionAvailable(CardPosition.Start)) return;

        // NEW: Get available cards not already on conveyor
        List<CardData> availableCards = deck
            .Where(card => !activeCardNames.Contains(card.cardName))
            .ToList();

        if (availableCards.Count == 0) return;

        int randomIndex = Random.Range(0, availableCards.Count);
        CardData cardData = availableCards[randomIndex];

        GameObject cardObj = Instantiate(cardPrefab, startPosition.position, Quaternion.identity, transform);
        DraggableCard card = cardObj.GetComponent<DraggableCard>();

        card.conveyorSystem = this;
        card.cardData = cardData;

        // Assign to start position
        AssignCardToPosition(card, CardPosition.Start);
        card.targetPosition = GetPositionForCard(CardPosition.Start);

        cardsOnConveyor.Add(card);
        activeCardNames.Add(cardData.cardName); // NEW: Track active card
        needsAdvancement = true;
    }

    
    private bool IsPositionAvailable(CardPosition position)
    {
        return positionOccupants[position] == null;
    }
    
    private void AssignCardToPosition(DraggableCard card, CardPosition position)
    {
        // Clear previous position
        if (cardPositions.ContainsKey(card))
        {
            CardPosition oldPosition = cardPositions[card];
            positionOccupants[oldPosition] = null;
        }
        
        // Assign new position
        cardPositions[card] = position;
        positionOccupants[position] = card;
        card.currentPosition = position;
    }
    
    private void MoveCardsOnConveyor()
    {
        // First, try to advance cards to next positions
        if (needsAdvancement) return;
        
        // Then move cards toward their target positions
        foreach (DraggableCard card in cardsOnConveyor.ToArray())
        {
            if (card == null) continue;
            if (card.isDragging) continue;
            
            card.transform.position = Vector3.MoveTowards(
                card.transform.position, 
                card.targetPosition, 
                conveyorSpeed * Time.deltaTime
            );
            
            // If card reached target, mark for advancement
            if (Vector3.Distance(card.transform.position, card.targetPosition) < 0.01f)
            {
                needsAdvancement = true;
            }
        }
    }
    
    private void TryAdvanceCards()
    {
        // Check if we can move cards forward
        bool advancementOccurred = false;
        
        // Try to move end position card out (shouldn't happen, but just in case)
        if (!IsPositionAvailable(CardPosition.End) && 
            positionOccupants[CardPosition.End] != null)
        {
            DraggableCard endCard = positionOccupants[CardPosition.End];
            if (endCard != null && endCard.currentPosition == CardPosition.End)
            {
                // Card at end stays until played
            }
        }
        
        // Try to move middle card to end
        if (!IsPositionAvailable(CardPosition.Middle) && 
            IsPositionAvailable(CardPosition.End))
        {
            DraggableCard middleCard = positionOccupants[CardPosition.Middle];
            if (middleCard != null && middleCard.currentPosition == CardPosition.Middle)
            {
                AdvanceCardToPosition(middleCard, CardPosition.End);
                advancementOccurred = true;
            }
        }
        
        // Try to move start card to middle
        if (!IsPositionAvailable(CardPosition.Start) && 
            IsPositionAvailable(CardPosition.Middle))
        {
            DraggableCard startCard = positionOccupants[CardPosition.Start];
            if (startCard != null && startCard.currentPosition == CardPosition.Start)
            {
                AdvanceCardToPosition(startCard, CardPosition.Middle);
                advancementOccurred = true;
            }
        }
        
        // If advancement occurred, we may need to check again
        if (advancementOccurred)
        {
            needsAdvancement = true;
        }
    }
    
    private void AdvanceCardToPosition(DraggableCard card, CardPosition newPosition)
    {
        AssignCardToPosition(card, newPosition);
        card.targetPosition = GetPositionForCard(newPosition);
    }
    
    private Vector3 GetPositionForCard(CardPosition position)
    {
        switch (position)
        {
            case CardPosition.Start: return startPosition.position;
            case CardPosition.Middle: return middlePosition.position;
            case CardPosition.End: return endPosition.position;
            default: return endPosition.position;
        }
    }

    public void RemoveCard(DraggableCard card)
    {
        if (card == null) return;

        // Remove from position tracking
        if (cardPositions.ContainsKey(card))
        {
            CardPosition position = cardPositions[card];
            positionOccupants[position] = null;
            cardPositions.Remove(card);
        }

        // NEW: Remove from active cards
        if (card.cardData != null)
        {
            activeCardNames.Remove(card.cardData.cardName);
        }

        cardsOnConveyor.Remove(card);
        Destroy(card.gameObject);

        needsAdvancement = true;
    }

    public bool TrySpendEnergy(float amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            return true;
        }
        return false;
    }
    
    public void DestroyCardForEnergy(DraggableCard card)
    {
        if (_isDestroyOnCooldown) return;
        
        float refund = card.cardData.energyCost * destroyRefundPercent;
        currentEnergy = Mathf.Min(maxEnergy, currentEnergy + refund);
        
        RemoveCard(card);
        _lastDestroyTime = Time.time;
        _isDestroyOnCooldown = true;
    }
    
    public bool CanDestroyCard()
    {
        return !_isDestroyOnCooldown;
    }
    
    public float GetCooldownProgress()
    {
        if (!_isDestroyOnCooldown) return 0f;
        return Mathf.Clamp01((Time.time - _lastDestroyTime) / destroyCooldown);
    }
    
    public bool IsDestroyOnCooldown()
    {
        return _isDestroyOnCooldown;
    }
}

public enum CardPosition { Start, Middle, End }

[System.Serializable]
public class CardData
{
    public string cardName;
    public CardType cardType;
    public int energyCost;
    public float damageBuffPercent;
    public float rangeBuffPercent;
    public float fireRateBuffPercent;
    public float buffDuration;
}

public enum CardType { TurretBuff, CityBuff, EnemyEffect }