using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Eğitim sırasında oyuncu rolünü oynayan basit bot
/// Dengeli strateji - AI'a öğrenme şansı tanır
/// </summary>
public class TrainingPlayer : MonoBehaviour
{
    private CardData[] availableCards;
    private int currentStamina = 7;
    private int maxStamina = 7;
    
    // Simüle kartlar (eğer deck boşsa)
    private List<SimulatedCard> simulatedCards;
    
    public void Initialize(CardData[] deck)
    {
        availableCards = deck;
        currentStamina = maxStamina;
        
        // Eğer deck boşsa, simüle kartlar oluştur
        if (deck == null || deck.Length == 0)
        {
            Debug.LogWarning("[TrainingPlayer] Deck boş! Simüle kartlar kullanılacak.");
            CreateSimulatedCards();
        }
    }

    private void CreateSimulatedCards()
    {
        simulatedCards = new List<SimulatedCard>
        {
            new SimulatedCard("Taco", 17, "Damage", 0, 1),
            new SimulatedCard("HotDog", 35, "Damage", 0, 1),
            new SimulatedCard("Sushi", 10, "Poison", 3, 1),
            new SimulatedCard("Lemon", 8, "Debuff", 2, 1),
            new SimulatedCard("CheeseBurger", 15, "Stun", 1, 3),
            new SimulatedCard("Donut", 10, "Debuff", 2, 1),
        };
    }

    public void ResetForNewEpisode()
    {
        currentStamina = maxStamina;
    }

    public void OnTurnStart()
    {
        currentStamina = Mathf.Min(currentStamina + 2, maxStamina);
    }

    /// <summary>
    /// Dengeli kart seçimi - AI'a öğrenme şansı tanır
    /// </summary>
    public CardData SelectCard(float playerHP, float customerHP, float maxHP,
                               int customerStunTurns, int customerPoisonStacks)
    {
        // Gerçek kartlar varsa onları kullan
        if (availableCards != null && availableCards.Length > 0)
        {
            return SelectRealCard(playerHP, customerHP, maxHP, customerStunTurns, customerPoisonStacks);
        }
        
        // Simüle kartlar kullan
        return SelectSimulatedCard(playerHP, customerHP, maxHP, customerStunTurns, customerPoisonStacks);
    }

    private CardData SelectRealCard(float playerHP, float customerHP, float maxHP,
                                    int customerStunTurns, int customerPoisonStacks)
    {
        List<CardData> affordableCards = availableCards
            .Where(c => c.staminaCost <= currentStamina)
            .ToList();

        if (affordableCards.Count == 0)
            return null;

        CardData selectedCard = null;
        float customerHPRatio = customerHP / maxHP;

        // Strateji (daha dengeli):
        // 1. %50 ihtimalle rastgele kart (AI'a şans tanı)
        // 2. Düşük HP ise en yüksek hasar
        // 3. %30 ihtimalle stun (her zaman değil!)
        // 4. Poison/debuff dene
        // 5. Rastgele

        float random = Random.value;

        // %50 rastgele seçim
        if (random < 0.5f)
        {
            selectedCard = affordableCards[Random.Range(0, affordableCards.Count)];
        }
        // Düşük HP - bitir
        else if (customerHPRatio <= 0.25f)
        {
            selectedCard = affordableCards.OrderByDescending(c => c.baseDamage).FirstOrDefault();
        }
        // %30 stun (sadece stunlanmamışsa)
        else if (customerStunTurns <= 0 && Random.value < 0.3f)
        {
            var stunCard = affordableCards.FirstOrDefault(c => c.effectType == "Stun");
            if (stunCard != null)
                selectedCard = stunCard;
        }
        // Poison dene
        else if (customerPoisonStacks <= 0 && Random.value < 0.4f)
        {
            var poisonCard = affordableCards.FirstOrDefault(c => c.effectType == "Poison");
            if (poisonCard != null)
                selectedCard = poisonCard;
        }
        
        // Fallback: Rastgele
        if (selectedCard == null)
        {
            selectedCard = affordableCards[Random.Range(0, affordableCards.Count)];
        }

        currentStamina -= selectedCard.staminaCost;
        return selectedCard;
    }

    private CardData SelectSimulatedCard(float playerHP, float customerHP, float maxHP,
                                         int customerStunTurns, int customerPoisonStacks)
    {
        if (simulatedCards == null || simulatedCards.Count == 0)
            return null;

        var affordable = simulatedCards.Where(c => c.staminaCost <= currentStamina).ToList();
        if (affordable.Count == 0)
            return null;

        // Simüle kartlarda daha basit strateji - çoğunlukla rastgele
        SimulatedCard selected;
        
        if (Random.value < 0.7f)
        {
            // %70 rastgele
            selected = affordable[Random.Range(0, affordable.Count)];
        }
        else
        {
            // %30 en yüksek hasarlı
            selected = affordable.OrderByDescending(c => c.damage).First();
        }

        currentStamina -= selected.staminaCost;
        
        // Simüle hasar uygula (CardData döndüremiyoruz, null dön ama hasarı simüle et)
        // Bu durumda AITrainer.SimulatePlayerTurn'de ele alınacak
        return null;
    }

    // Simüle kart için hasar hesapla (CardData yokken)
    public (float damage, string effect, int duration) GetSimulatedAction(float customerHP, float maxHP, 
                                                                           int customerStunTurns, int customerPoisonStacks)
    {
        if (simulatedCards == null || simulatedCards.Count == 0)
            return (15f, "Damage", 0); // Default

        var affordable = simulatedCards.Where(c => c.staminaCost <= currentStamina).ToList();
        if (affordable.Count == 0)
            return (0f, "None", 0);

        SimulatedCard selected;
        
        // %60 rastgele, %40 stratejik
        if (Random.value < 0.6f)
        {
            selected = affordable[Random.Range(0, affordable.Count)];
        }
        else if (customerStunTurns <= 0 && Random.value < 0.25f)
        {
            selected = affordable.FirstOrDefault(c => c.effect == "Stun") ?? affordable[0];
        }
        else
        {
            selected = affordable.OrderByDescending(c => c.damage).First();
        }

        currentStamina -= selected.staminaCost;
        return (selected.damage, selected.effect, selected.duration);
    }

    public int GetCurrentStamina() => currentStamina;
    public bool HasRealCards() => availableCards != null && availableCards.Length > 0;

    // Basit simüle kart yapısı
    private class SimulatedCard
    {
        public string name;
        public float damage;
        public string effect;
        public int duration;
        public int staminaCost;

        public SimulatedCard(string n, float d, string e, int dur, int cost)
        {
            name = n; damage = d; effect = e; duration = dur; staminaCost = cost;
        }
    }
}

