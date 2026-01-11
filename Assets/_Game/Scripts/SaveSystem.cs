using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Oyun kayıt sistemi - Level, Coins, Unlocked Cards, Equipped Cards, AI Mode
/// </summary>
public static class SaveSystem
{
    private const string KEY_CURRENT_LEVEL = "CurrentLevel";
    private const string KEY_COINS = "Coins";
    private const string KEY_UNLOCKED_CARDS = "UnlockedCards";
    private const string KEY_EQUIPPED_CARDS = "EquippedCards";
    private const string KEY_AI_LOADED = "AILoaded";
    
    public const int MAX_EQUIPPED_CARDS = 8;
    public const int MIN_EQUIPPED_CARDS = 8;

    // ============ LEVEL ============
    public static int CurrentLevel
    {
        get => PlayerPrefs.GetInt(KEY_CURRENT_LEVEL, 1);
        set
        {
            PlayerPrefs.SetInt(KEY_CURRENT_LEVEL, value);
            PlayerPrefs.Save();
        }
    }

    // ============ COINS ============
    public static int Coins
    {
        get => PlayerPrefs.GetInt(KEY_COINS, 0);
        set
        {
            PlayerPrefs.SetInt(KEY_COINS, value);
            PlayerPrefs.Save();
        }
    }

    public static void AddCoins(int amount)
    {
        Coins += amount;
        Debug.Log($"[SaveSystem] +{amount} coins. Total: {Coins}");
    }

    public static bool SpendCoins(int amount)
    {
        if (Coins >= amount)
        {
            Coins -= amount;
            Debug.Log($"[SaveSystem] -{amount} coins. Remaining: {Coins}");
            return true;
        }
        Debug.Log($"[SaveSystem] Not enough coins! Need {amount}, have {Coins}");
        return false;
    }

    // ============ UNLOCKED CARDS ============
    private static HashSet<string> _unlockedCards;
    
    private static HashSet<string> UnlockedCards
    {
        get
        {
            if (_unlockedCards == null)
            {
                _unlockedCards = new HashSet<string>();
                string data = PlayerPrefs.GetString(KEY_UNLOCKED_CARDS, "");
                if (!string.IsNullOrEmpty(data))
                {
                    string[] cards = data.Split(',');
                    foreach (string card in cards)
                    {
                        if (!string.IsNullOrEmpty(card))
                            _unlockedCards.Add(card);
                    }
                }
            }
            return _unlockedCards;
        }
    }

    public static bool IsCardUnlocked(string cardName)
    {
        return UnlockedCards.Contains(cardName);
    }

    public static void UnlockCard(string cardName)
    {
        if (!UnlockedCards.Contains(cardName))
        {
            UnlockedCards.Add(cardName);
            SaveUnlockedCards();
            Debug.Log($"[SaveSystem] Card unlocked: {cardName}");
        }
    }

    private static void SaveUnlockedCards()
    {
        string data = string.Join(",", UnlockedCards);
        PlayerPrefs.SetString(KEY_UNLOCKED_CARDS, data);
        PlayerPrefs.Save();
    }

    // ============ EQUIPPED CARDS ============
    private static HashSet<string> _equippedCards;
    
    private static HashSet<string> EquippedCards
    {
        get
        {
            if (_equippedCards == null)
            {
                _equippedCards = new HashSet<string>();
                string data = PlayerPrefs.GetString(KEY_EQUIPPED_CARDS, "");
                if (!string.IsNullOrEmpty(data))
                {
                    string[] cards = data.Split(',');
                    foreach (string card in cards)
                    {
                        if (!string.IsNullOrEmpty(card))
                            _equippedCards.Add(card);
                    }
                }
            }
            return _equippedCards;
        }
    }

    public static bool IsCardEquipped(string cardName)
    {
        return EquippedCards.Contains(cardName);
    }

    public static int GetEquippedCount()
    {
        return EquippedCards.Count;
    }

    public static bool CanEquipMore()
    {
        return EquippedCards.Count < MAX_EQUIPPED_CARDS;
    }

    public static bool HasMinimumEquipped()
    {
        return EquippedCards.Count >= MIN_EQUIPPED_CARDS;
    }

    public static bool EquipCard(string cardName)
    {
        // Not: Unlock kontrolü UI tarafında yapılıyor (CardData.IsUnlocked)
        // SaveSystem unlockCost bilgisine erişemez, bu yüzden burada kontrol yapmıyoruz
        
        if (EquippedCards.Count >= MAX_EQUIPPED_CARDS)
        {
            Debug.Log($"[SaveSystem] Max equipped cards reached ({MAX_EQUIPPED_CARDS})");
            return false;
        }
        
        if (!EquippedCards.Contains(cardName))
        {
            EquippedCards.Add(cardName);
            SaveEquippedCards();
            Debug.Log($"[SaveSystem] Card equipped: {cardName} ({EquippedCards.Count}/{MAX_EQUIPPED_CARDS})");
            return true;
        }
        return false;
    }

    public static bool UnequipCard(string cardName)
    {
        // Not: Minimum 8 kart kontrolü PLAY butonunda yapılıyor
        // Cards menüsünde serbestçe equip/unequip yapılabilir
        
        if (EquippedCards.Contains(cardName))
        {
            EquippedCards.Remove(cardName);
            SaveEquippedCards();
            Debug.Log($"[SaveSystem] Card unequipped: {cardName} ({EquippedCards.Count}/{MAX_EQUIPPED_CARDS})");
            return true;
        }
        return false;
    }

    public static List<string> GetEquippedCardNames()
    {
        return new List<string>(EquippedCards);
    }

    private static void SaveEquippedCards()
    {
        string data = string.Join(",", EquippedCards);
        PlayerPrefs.SetString(KEY_EQUIPPED_CARDS, data);
        PlayerPrefs.Save();
    }

    // ============ AI MODE ============
    public static bool AILoaded
    {
        get => PlayerPrefs.GetInt(KEY_AI_LOADED, 0) == 1;
        set
        {
            PlayerPrefs.SetInt(KEY_AI_LOADED, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    // ============ RESET ALL ============
    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(KEY_CURRENT_LEVEL);
        PlayerPrefs.DeleteKey(KEY_COINS);
        PlayerPrefs.DeleteKey(KEY_UNLOCKED_CARDS);
        PlayerPrefs.DeleteKey(KEY_EQUIPPED_CARDS);
        PlayerPrefs.DeleteKey(KEY_AI_LOADED);
        
        // Also reset AI Q-Table
        PlayerPrefs.DeleteKey("CustomerAI_QTable");
        PlayerPrefs.DeleteKey("CustomerAI_Episodes");
        PlayerPrefs.DeleteKey("CustomerAI_ExplorationRate");
        
        _unlockedCards = null;
        _equippedCards = null;
        PlayerPrefs.Save();
        
        Debug.Log("[SaveSystem] All data reset!");
    }
}

