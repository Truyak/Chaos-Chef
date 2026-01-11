using UnityEngine;

[CreateAssetMenu]
public class CardData : ScriptableObject
{
    public string cardName;
    public Sprite artwork;
    public float baseDamage = 30f;
    public string effectType; // "Damage", "Stun", "Poison", "Debuff", "Heal"
    public int duration;
    public string description;
    
    [Header("Stamina System")]
    public int staminaCost = 1; // Stun kartları için 3, Cold Soup için 4
    
    [Header("Special Effects")]
    public bool isHeal = false; // Heal kartları için true
    
    [Header("Unlock System")]
    public int unlockCost = 0; // 0 = baştan açık, >0 = kilitli ve coin ile açılır
    
    /// <summary>
    /// Kartın açık olup olmadığını kontrol eder
    /// </summary>
    public bool IsUnlocked()
    {
        // Baştan açık kartlar (unlockCost = 0)
        if (unlockCost <= 0) return true;
        
        // SaveSystem'den kontrol et
        return SaveSystem.IsCardUnlocked(cardName);
    }
    
    /// <summary>
    /// Kartı aç (coin harcayarak)
    /// </summary>
    public bool TryUnlock()
    {
        if (IsUnlocked()) return true;
        
        if (SaveSystem.SpendCoins(unlockCost))
        {
            SaveSystem.UnlockCard(cardName);
            return true;
        }
        return false;
    }
}