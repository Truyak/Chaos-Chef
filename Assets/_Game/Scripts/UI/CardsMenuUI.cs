using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Cards menüsü - Kart koleksiyonu, unlock ve equip sistemi
/// </summary>
public class CardsMenuUI : MonoBehaviour
{
    [Header("References")]
    public MainMenuManager mainMenuManager;
    public Transform cardGridParent;
    public GameObject cardSlotPrefab;
    
    [Header("Available Cards")]
    public CardData[] allCards;
    
    [Header("Detail Panel")]
    public GameObject detailPanel;
    public Image detailCardImage;
    public Image cardPriceImage;
    public TextMeshProUGUI cardPriceText;
    public Button unlockButton;
    public TextMeshProUGUI unlockButtonText;
    public Button equipButton;
    public TextMeshProUGUI equipButtonText;
    public Image equipButtonImage;
    
    [Header("Header")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI equippedCountText;
    public Button backButton;
    
    private CardData selectedCard;
    private List<GameObject> cardSlots = new List<GameObject>();
    
    private void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(OnBackPressed);
        if (unlockButton != null)
            unlockButton.onClick.AddListener(OnUnlockPressed);
        if (equipButton != null)
            equipButton.onClick.AddListener(OnEquipPressed);
        
        PopulateCardGrid();
        UpdateUI();
        
        if (detailPanel != null)
            detailPanel.SetActive(false);
    }
    
    private void OnEnable()
    {
        PopulateCardGrid();
        UpdateUI();
    }
    
    public void UpdateUI()
    {
        if (coinsText != null)
            coinsText.text = $"Coins: {SaveSystem.Coins}";
        
        if (equippedCountText != null)
            equippedCountText.text = $"{SaveSystem.GetEquippedCount()}/{SaveSystem.MAX_EQUIPPED_CARDS}";
    }
    
    public void PopulateCardGrid()
    {
        // Clear existing slots
        foreach (var slot in cardSlots)
        {
            if (slot != null) Destroy(slot);
        }
        cardSlots.Clear();
        
        if (allCards == null || cardSlotPrefab == null || cardGridParent == null)
            return;
        
        foreach (CardData card in allCards)
        {
            GameObject slot = Instantiate(cardSlotPrefab, cardGridParent);
            cardSlots.Add(slot);
            
            // Get components
            Image cardImage = slot.transform.Find("CardImage")?.GetComponent<Image>();
            GameObject lockOverlay = slot.transform.Find("LockOverlay")?.gameObject;
            TextMeshProUGUI costText = slot.transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
            GameObject equippedIndicator = slot.transform.Find("EquippedIndicator")?.gameObject;
            Button slotButton = slot.GetComponent<Button>();
            TextMeshProUGUI elixirAmount = slot.transform.Find("elixirImage")?.transform.Find("staminaCostText")?.GetComponent<TextMeshProUGUI>();
            if (elixirAmount != null)
                elixirAmount.text = card.staminaCost.ToString();
            else
                Debug.LogWarning($"[CardsMenuUI] staminaCostText not found in card slot prefab.");

            bool isUnlocked = card.IsUnlocked();
            bool isEquipped = SaveSystem.IsCardEquipped(card.cardName);
            
            // Setup card image
            if (cardImage != null && card.artwork != null)
                cardImage.sprite = card.artwork;
            
            // Lock overlay
            if (lockOverlay != null)
                lockOverlay.SetActive(!isUnlocked);
            if (costText != null)
                costText.text = isUnlocked ? "" : $"{card.unlockCost}";
            
            // Equipped indicator (opsiyonel - varsa göster)
            if (equippedIndicator != null)
                equippedIndicator.SetActive(isEquipped);
            
            // Card visual states
            if (cardImage != null)
            {
                if (!isUnlocked)
                    cardImage.color = new Color(0.4f, 0.4f, 0.4f, 1f); // Locked - dark
                else if (isEquipped)
                    cardImage.color = Color.white; // Equipped - full bright
                else
                    cardImage.color = new Color(0.75f, 0.75f, 0.75f, 1f); // Unlocked but not equipped - slightly dim
            }
            
            // Click handler - detail panel aç
            CardData cardRef = card;
            if (slotButton != null)
                slotButton.onClick.AddListener(() => ShowCardDetail(cardRef));
        }
    }
    
    public void ShowCardDetail(CardData card)
    {
        selectedCard = card;
        
        if (detailPanel != null)
            detailPanel.SetActive(true);
        
        if (detailCardImage != null && card.artwork != null)
            detailCardImage.sprite = card.artwork;
        
        bool isUnlocked = card.IsUnlocked();
        bool isEquipped = SaveSystem.IsCardEquipped(card.cardName);
        
        // Price display
        if (cardPriceText != null)
        {
            if (isEquipped)
                cardPriceText.text = "EQUIPPED";
            else if (isUnlocked)
                cardPriceText.text = "UNLOCKED";
            else
                cardPriceText.text = $"{card.unlockCost}";
        }
        
        if (cardPriceImage != null)
            cardPriceImage.gameObject.SetActive(!isUnlocked);

        // Unlock button - sadece kilitliyse göster
        if (unlockButton != null)
            unlockButton.gameObject.SetActive(!isUnlocked);
        
        if (unlockButtonText != null)
            unlockButtonText.text = $"UNLOCK ({card.unlockCost})";
        
        // Equip button - sadece açıksa göster
        if (equipButton != null)
        {
            equipButton.gameObject.SetActive(isUnlocked);
            equipButton.interactable = true; // Her zaman aktif - serbestçe değiştirilebilir
            
            if (isEquipped)
            {
                // EQUIPPED - gri pasif görünüm
                if (equipButtonText != null)
                    equipButtonText.text = "EQUIPPED";
                if (equipButtonImage != null)
                    equipButtonImage.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            }
            else
            {
                // EQUIP - yeşil aktif görünüm
                if (equipButtonText != null)
                    equipButtonText.text = "EQUIP";
                if (equipButtonImage != null)
                    equipButtonImage.color = new Color(0.2f, 0.8f, 0.3f, 1f);
            }
        }
    }
    
    public void OnUnlockPressed()
    {
        if (selectedCard == null) return;
        
        if (selectedCard.TryUnlock())
        {
            UpdateUI();
            PopulateCardGrid();
            ShowCardDetail(selectedCard); // Refresh - artık equip butonu görünecek
        }
        else
        {
            Debug.Log("[CardsMenu] Not enough coins!");
        }
    }
    
    public void OnEquipPressed()
    {
        if (selectedCard == null) return;
        
        bool isEquipped = SaveSystem.IsCardEquipped(selectedCard.cardName);
        
        if (isEquipped)
        {
            SaveSystem.UnequipCard(selectedCard.cardName);
        }
        else
        {
            SaveSystem.EquipCard(selectedCard.cardName);
        }
        
        UpdateUI();
        PopulateCardGrid();
        ShowCardDetail(selectedCard); // Refresh
    }
    
    public void OnBackPressed()
    {
        if (detailPanel != null && detailPanel.activeSelf)
        {
            detailPanel.SetActive(false);
        }
        else if (mainMenuManager != null)
        {
            mainMenuManager.OnBackToMainMenu();
        }
    }
    
    public void CloseDetailPanel()
    {
        if (detailPanel != null)
            detailPanel.SetActive(false);
    }
}
