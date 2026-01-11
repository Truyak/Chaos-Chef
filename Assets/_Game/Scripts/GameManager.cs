using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Cards")]
    public CardData[] startingDeck = new CardData[6];

    private Queue<CardData> drawPile = new();

    [Header("UI")]
    public Transform deckPanel;
    public GameObject cardPrefab;
    public Slider playerHPSlider;
    public TextMeshProUGUI playerHPText;
    public Slider customerHPSlider;
    public TextMeshProUGUI customerHPText;
    public TextMeshProUGUI winLoseText;
    public TextMeshProUGUI winCoinText;
    public GameObject winLosePanel;
    public TextMeshProUGUI turnText;
    public TurnTimelineUI turnTimeline; // [NEW] Timeline referansı
    public Button AITurnButton;
    public Sprite StunSprite;
    public Sprite DebuffSprite;
    public Sprite PoisonSprite;
    public GameObject customerSymbolPanel;
    public GameObject waiterSymbolPanel;
    private GameObject customerStunImage;
    private GameObject customerDebuffImage;
    private GameObject playerStunImage;
    private GameObject playerDebuffImage;
    private GameObject customerPoisonImage;
    private GameObject playerPoisonImage;

    [Header("Values")]
    public float playerHP = 100;
    public float customerHP = 100;
    public float maxHP = 100;

    public bool isPlayerTurn = true;

    private int playerPoisonStacks = 0;
    private int playerStunTurns = 0;
    private int playerDebuffTurns = 0;
    private float playerDebuffMultiplier = 1f;

    private int customerPoisonStacks = 0;
    private int customerStunTurns = 0;
    private int customerDebuffTurns = 0;
    private float customerDebuffMultiplier = 1f;

    [Header("Stamina System")]
    public int playerStamina = 7;
    public int maxStamina = 7;
    public int staminaPerTurn = 2;
    public TextMeshProUGUI staminaText; // UI için
    public Slider staminaSlider; // Opsiyonel slider

    [Header("AI System")]
    private CustomerAI customerAI;

    [Header("Customer System")]
    private CustomerSpawner customerSpawner;
    private CustomerData currentCustomer;
    private int customerTurnCounter = 0; // Food Blogger extra turn için
    private int currentCustomerIndex = 0; // Progression için - SaveSystem'den yüklenecek
    private bool lastMatchWon = false;
    
    [Header("All Cards (for equip system)")]
    public CardData[] allCards; // Tüm kartlar - equipped kartları aramak için

    [Header("Waiter (Player) Animation")]
    public Animator waiterAnimator; // Inspector'dan ata

    [Header("Animation Control")]
    private bool isAnimationPlaying = false; // Animasyon oynarken aksiyon engelle
    private bool isExtraTurnAction = false; // Extra turn aksiyon için flag

    [Header("Game Over Panel")]
    public Button continueButton; // Next Enemy / Try Again butonu
    public TextMeshProUGUI continueButtonText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        winLosePanel.SetActive(false);
    }

    private void Start()
    {
        //playerHPSlider.maxValue = maxHP;
        //customerHPSlider.maxValue = maxHP;
        UpdateHPUI();
        winLosePanel.SetActive(false);
        AITurnButton.interactable = true;

        // Stamina setup
        playerStamina = maxStamina;
        if (staminaSlider != null) staminaSlider.maxValue = maxStamina;
        UpdateStaminaUI();
        
        // Level'i SaveSystem'den yükle (1-indexed -> 0-indexed)
        currentCustomerIndex = SaveSystem.CurrentLevel - 1;
        Debug.Log($"[GameManager] Starting at level {SaveSystem.CurrentLevel} (customer index {currentCustomerIndex})");

        // AI setup - SaveSystem'den mod kontrolü
        customerAI = CustomerAI.Instance;
        if (customerAI != null)
        {
            if (SaveSystem.AILoaded)
            {
                customerAI.LoadQTable();
                Debug.Log("[GameManager] Smart AI mode - Q-Table loaded");
            }
            else
            {
                customerAI.ResetQTable();
                customerAI.SetTrainingMode(true); // Rastgele mod için yüksek exploration
                Debug.Log("[GameManager] Random AI mode - Fresh Q-Table");
            }
        }

        // Customer Spawner setup
        customerSpawner = CustomerSpawner.Instance;
        if (customerSpawner != null)
        {
            customerSpawner.OnCustomerSpawned += OnCustomerSpawned;
        }

        SetupImages();
        SpawnDeck();
        UpdateTurnText();
        UpdateTimeline();
    }


    /// <summary>
    /// Yeni müşteri spawn edildiğinde çağrılır
    /// </summary>
    public void OnCustomerSpawned(CustomerData customer)
    {
        currentCustomer = customer;
        customerHP = customer.maxHP;
        maxHP = Mathf.Max(maxHP, customer.maxHP);
        
        if (turnTimeline != null && customer.icon != null)
        {
            turnTimeline.SetCustomerIcon(customer.icon);
            UpdateTimeline();
        }

        playerHPSlider.maxValue = playerHP;
        customerHPSlider.maxValue = customer.maxHP;
        Debug.Log($"[GameManager] Yeni musteri spawnlandi: {customer.customerName} HP: {customer.maxHP} + " + customerHPSlider.maxValue);
        UpdateHPUI();
    }

    void SetupImages()
    {
        customerStunImage = new GameObject("CustomerStunImage", typeof(Image));
        customerStunImage.GetComponent<Image>().sprite = StunSprite;
        customerStunImage.transform.SetParent(customerSymbolPanel.transform, false);
        customerStunImage.gameObject.SetActive(false);

        customerDebuffImage = new GameObject("CustomerDebuffImage", typeof(Image));
        customerDebuffImage.GetComponent<Image>().sprite = DebuffSprite;
        customerDebuffImage.transform.SetParent(customerSymbolPanel.transform, false);
        customerDebuffImage.gameObject.SetActive(false);

        playerStunImage = new GameObject("PlayerStunImage", typeof(Image));
        playerStunImage.GetComponent<Image>().sprite = StunSprite;
        playerStunImage.transform.SetParent(waiterSymbolPanel.transform, false);
        playerStunImage.gameObject.SetActive(false);

        playerDebuffImage = new GameObject("PlayerDebuffImage", typeof(Image));
        playerDebuffImage.GetComponent<Image>().sprite = DebuffSprite;
        playerDebuffImage.transform.SetParent(waiterSymbolPanel.transform, false);
        playerDebuffImage.gameObject.SetActive(false);

        customerPoisonImage = new GameObject("CustomerPoisonImage", typeof(Image));
        customerPoisonImage.GetComponent<Image>().sprite = PoisonSprite;
        customerPoisonImage.transform.SetParent(customerSymbolPanel.transform, false);
        customerPoisonImage.gameObject.SetActive(false);

        playerPoisonImage = new GameObject("PlayerPoisonImage", typeof(Image));
        playerPoisonImage.GetComponent<Image>().sprite = PoisonSprite;
        playerPoisonImage.transform.SetParent(waiterSymbolPanel.transform, false);
        playerPoisonImage.gameObject.SetActive(false);
    }

    void SpawnDeck()
    {
        // Equipped kartları al, yoksa starting deck kullan
        CardData[] deckToUse = GetEquippedDeck();
        
        deckToUse = ShuffleArray(deckToUse);
        drawPile.Clear();

        for (int i = 0; i < 4; i++)
        {
            if (i < deckToUse.Length)
            {
                GameObject newCard = Instantiate(cardPrefab, deckPanel);
                newCard.GetComponent<CardDisplay>().Setup(deckToUse[i]);
            }
        }

        for (int i = 4; i < deckToUse.Length; i++)
        {
            drawPile.Enqueue(deckToUse[i]);
        }
    }
    
    /// <summary>
    /// Equipped kartları döndürür, yoksa starting deck kullanır
    /// </summary>
    CardData[] GetEquippedDeck()
    {
        List<string> equippedNames = SaveSystem.GetEquippedCardNames();
        
        // Eğer hiç equipped kart yoksa, starting deck kullan
        if (equippedNames.Count == 0)
            return startingDeck;
        
        // Kart havuzu: önce allCards, yoksa startingDeck kullan
        CardData[] cardPool = (allCards != null && allCards.Length > 0) ? allCards : startingDeck;
        
        // Equipped kartları bul
        List<CardData> equippedCards = new List<CardData>();
        foreach (string cardName in equippedNames)
        {
            foreach (CardData card in cardPool)
            {
                if (card.cardName == cardName)
                {
                    equippedCards.Add(card);
                    break;
                }
            }
        }
        
        Debug.Log($"[GameManager] Equipped deck: {equippedCards.Count} cards");
        
        // Eğer bulunamadıysa starting deck kullan
        if (equippedCards.Count == 0)
            return startingDeck;
        
        return equippedCards.ToArray();
    }

    private CardData[] ShuffleArray(CardData[] deck)
    {
        for (int i = 0; i < deck.Length; i++)
        {
            CardData temp = deck[i];
            int randomIndex = Random.Range(i, deck.Length);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
        return deck;
    }

    public void PlayCard(CardDisplay playedCardDisplay)
    {
        if (!isPlayerTurn || Time.timeScale == 0f || isAnimationPlaying) return;

        CardData card = playedCardDisplay.cardData;

        // Stamina kontrolü
        if (playerStamina < card.staminaCost)
        {
            Debug.Log($"Yetersiz stamina! Gerekli: {card.staminaCost}, Mevcut: {playerStamina}");
            return;
        }

        // Kart bilgilerini coroutine'e geçir ve başlat
        StartCoroutine(PlayCardSequence(playedCardDisplay, card));
    }

    /// <summary>
    /// Kart oynama işlemini senkronize animasyonlarla gerçekleştirir
    /// </summary>
    private IEnumerator PlayCardSequence(CardDisplay playedCardDisplay, CardData card)
    {
        isAnimationPlaying = true;
        AITurnButton.interactable = false;

        // Kartı hemen gizle (animasyon sonunda yeni kartla görünecek)
        playedCardDisplay.gameObject.SetActive(false);

        // Stamina harca
        playerStamina -= card.staminaCost;
        UpdateStaminaUI();

        // 1. Waiter Attack animasyonu başlat
        if (waiterAnimator != null)
            waiterAnimator.SetTrigger("Attack");

        // 2. Attack animasyonunun vuruş anına kadar bekle
        yield return new WaitForSeconds(AnimationTimings.ATTACK_HIT_DELAY);

        // 3. Hasar veya iyileştirme uygula
        if (card.isHeal)
        {
            playerHP = Mathf.Min(playerHP + card.baseDamage, maxHP);
            AudioManager.Instance.PlayWhoop();
        }
        else
        {
            // Oyuncu debuff yediğinde (playerDebuffMultiplier < 1), oyuncunun verdiği hasar azalır
            float damage = Mathf.Round(card.baseDamage * playerDebuffMultiplier);
            customerHP -= damage;
            
            Debug.Log($"[PlayCard] {card.cardName}: {card.baseDamage} base × {playerDebuffMultiplier} debuff = {damage} gerçek hasar. Customer HP: {customerHP}");
            
            // 4. Müşteri Hit animasyonu - SADECE Stun değilse
            if (customerSpawner != null && card.effectType != "Stun")
                customerSpawner.PlayAnimation("Hit");
        }

        // 5. Efekt uygula
        switch (card.effectType)
        {
            case "Poison":
                customerPoisonStacks += card.duration;
                customerPoisonImage.gameObject.SetActive(true);
                AudioManager.Instance.PlayDamage();
                break;

            case "Stun":
                customerStunTurns += card.duration;
                customerStunImage.gameObject.SetActive(true);
                AudioManager.Instance.PlayStun();
                // Müşteri Stun animasyonu
                if (customerSpawner != null)
                    customerSpawner.PlayAnimation("Stun");
                UpdateTimeline(); // [NEW] Stun değişti, timeline güncelle
                break;

            case "Debuff":
                customerDebuffMultiplier = 0.4f;
                customerDebuffImage.gameObject.SetActive(true);
                customerDebuffTurns = card.duration;
                AudioManager.Instance.PlayWhoop();
                break;

            case "Damage":
                AudioManager.Instance.PlayDamage();
                break;

            case "Heal":
                // Zaten yukarıda işlendi
                break;
        }

        // 6. Hit animasyonu bitene kadar bekle
        yield return new WaitForSeconds(AnimationTimings.HIT_REACTION);

        // 7. Kart havuzunu güncelle ve yeni kartı görünür yap
        drawPile.Enqueue(card);
        CardData nextCard = drawPile.Dequeue();
        playedCardDisplay.Setup(nextCard);
        playedCardDisplay.gameObject.SetActive(true); // Yeni kartla görünür yap

        UpdateHPUI();
        
        // 8. Win/Lose kontrolü
        CheckWinLose();

        isAnimationPlaying = false;
        AITurnButton.interactable = true;

        // NOT: Artık burada AIPlayerTurn() çağrılmıyor!
        // Oyuncu istediği kadar kart oynayabilir, sonra "Turu Bitir" butonuna basar
        Debug.Log($"[PlayCard] {card.cardName} oynandı. Kalan stamina: {playerStamina}");
    }

    /// <summary>
    /// Oyuncu turunu bitirir - "Turu Bitir" butonuna bağlanacak
    /// </summary>
    public void EndPlayerTurn()
    {
        if (!isPlayerTurn || isAnimationPlaying) return;
        
        Debug.Log("[EndPlayerTurn] Oyuncu turu bitti, AI sırası.");
        AIPlayerTurn();
    }

    public void AIPlayerTurn()
    {
        isPlayerTurn = false;
        AITurnButton.interactable = false; // Müşteri saldırırken buton pasif

        HandleTurnStart(false);
    }

    public void FinishCustomerTurn()
    {
        // Food Blogger extra turn kontrolü
        if (currentCustomer != null && currentCustomer.hasExtraTurn)
        {
            customerTurnCounter++;
            
            // Her X turda bir ekstra aksiyon
            if (customerTurnCounter % currentCustomer.extraTurnInterval == 0)
            {
                Debug.Log($"[Food Blogger] EKSTRA TUR! ({customerTurnCounter}. tur)");
                // Ekstra aksiyon yap ama sonra oyuncuya geç
                StartCoroutine(ExtraTurnAction());
                return;
            }
        }
        
        isPlayerTurn = true;
        AITurnButton.interactable = true; // Oyuncu turu başladı, buton aktif

        HandleTurnStart(true);
    }

    private IEnumerator ExtraTurnAction()
    {
        yield return new WaitForSeconds(AnimationTimings.TURN_TRANSITION_DELAY);
        
        // Extra turn flag'ı ayarla - CustomerActionSequence içinde FinishCustomerTurn çağrılmayacak
        isExtraTurnAction = true;
        
        // Ekstra saldırı yap
        CustomerAction();
        
        // CustomerActionSequence tamamlanana kadar bekle
        yield return new WaitWhile(() => isAnimationPlaying);
        
        isExtraTurnAction = false;
        
        // Sonra normal şekilde oyuncuya geç
        isPlayerTurn = true;
        AITurnButton.interactable = true;
        HandleTurnStart(true);
    }

    void HandleTurnStart(bool isPlayerNext)
    {
        UpdateTurnText();
        UpdateTimeline(); // [NEW] Tur değişti, timeline güncelle
        if (Time.timeScale == 0f) return;

        if (isPlayerNext)
        {
            // Stamina yenilenme (tur başı)
            playerStamina = Mathf.Min(playerStamina + staminaPerTurn, maxStamina);
            UpdateStaminaUI();

            if (playerPoisonStacks > 0)
            {
                playerHP -= 10f;
                playerPoisonStacks--;
                if(playerPoisonStacks <= 0) playerPoisonImage.gameObject.SetActive(false);
            }

            if (playerDebuffTurns > 0)
            {
                playerDebuffTurns--;
                if (playerDebuffTurns <= 0)
                {
                    playerDebuffMultiplier = 1f;
                    playerDebuffImage.gameObject.SetActive(false);
                }
            }

            if (playerStunTurns > 0)
            {
                playerStunTurns--;
                if (playerStunTurns <= 0) playerStunImage.gameObject.SetActive(false);
                Debug.Log("Oyuncu STUN yediği için turu pas geçiyor!");
                UpdateHPUI();
                CheckWinLose();

                if (playerHP > 0) Invoke("AIPlayerTurn", 1.5f);
                return;
            }
        }
        else
        {
            if (customerPoisonStacks > 0)
            {
                customerHP -= 10f;
                customerPoisonStacks--;
                if (customerPoisonStacks <= 0) customerPoisonImage.gameObject.SetActive(false);
            }

            if (customerDebuffTurns > 0)
            {
                customerDebuffTurns--;
                if (customerDebuffTurns <= 0)
                {
                    customerDebuffMultiplier = 1f;
                    customerDebuffImage.gameObject.SetActive(false);
                }
            }

            if (customerStunTurns > 0)
            {
                customerStunTurns--;
                if (customerStunTurns <= 0) customerStunImage.gameObject.SetActive(false);
                UpdateHPUI();
                CheckWinLose();

                if (customerHP > 0) FinishCustomerTurn();
                return;
            }

            // Müşteri stunned değilse, saldırısını yapsın
            if (customerHP > 0)
            {
                isPlayerTurn = false;
                AITurnButton.interactable = true;
                CustomerAction(); // AI aksiyonunu çağır!
                return; // CustomerAction zaten FinishCustomerTurn() çağırıyor
            }
        }

        UpdateHPUI();
        CheckWinLose();
    }

    public void CustomerAction()
    {
        StartCoroutine(CustomerActionSequence());
    }

    /// <summary>
    /// Müşteri aksiyonunu senkronize animasyonlarla gerçekleştirir
    /// </summary>
    private IEnumerator CustomerActionSequence()
    {
        isAnimationPlaying = true;
        AITurnButton.interactable = false;

        // Q-Learning AI kullan
        if (customerAI != null)
        {
            // State hesapla
            int state = customerAI.GetStateHash(
                playerHP, customerHP, maxHP,
                playerStunTurns, playerPoisonStacks, playerDebuffTurns
            );
            Debug.Log("CustomerAI exists.");
            // Aksiyon seç
            int actionIndex = customerAI.SelectAction(state);
            AIAction action = customerAI.ExecuteAction(actionIndex);

            // 1. Müşteri Attack animasyonu başlat
            if (customerSpawner != null)
                customerSpawner.PlayAnimation("Attack");

            // 2. Attack animasyonunun vuruş anına kadar bekle
            yield return new WaitForSeconds(AnimationTimings.ATTACK_HIT_DELAY);

            // 3. Aksiyonu uygula - AI'ın hasarı customerDebuffMultiplier ile azaltılır
            float damage = Mathf.Round(action.damage * customerDebuffMultiplier);
            playerHP -= damage;

            // 4. Waiter Hit animasyonu (hasar aldığında)
            if (damage > 0 && waiterAnimator != null)
                waiterAnimator.SetTrigger("Hit");

            switch (action.effectType)
            {
                case "Stun":
                    playerStunTurns += action.effectDuration;
                    playerStunImage.gameObject.SetActive(true);
                    AudioManager.Instance.PlayAllergy();
                    // Waiter Stun animasyonu
                    if (waiterAnimator != null)
                        waiterAnimator.SetTrigger("Stun");
                    UpdateTimeline(); // [NEW] Stun değişti, timeline güncelle
                    break;
                case "Poison":
                    playerPoisonStacks += action.effectDuration;
                    playerPoisonImage.gameObject.SetActive(true);
                    AudioManager.Instance.PlayBadComments();
                    break;
                case "Debuff":
                    playerDebuffMultiplier = 0.6f;
                    playerDebuffImage.gameObject.SetActive(true);
                    playerDebuffTurns = action.effectDuration;
                    AudioManager.Instance.PlayShock();
                    break;
                default:
                    if (damage > 20)
                        AudioManager.Instance.PlayComplaints();
                    else
                        AudioManager.Instance.PlayShock();
                    break;
            }

            Debug.Log($"Musteri: {action.actionName}! {damage} hasar, Efekt: {action.effectType}");

            // Reward hesapla ve Q-Table güncelle
            float reward = customerAI.CalculateReward(damage, action.effectType);
            int newState = customerAI.GetStateHash(
                playerHP, customerHP, maxHP,
                playerStunTurns, playerPoisonStacks, playerDebuffTurns
            );
            customerAI.UpdateQTable(newState, reward, false);
            customerAI.TickAllCooldowns();
        }
        else
        {
            // Fallback: Eski random sistem
            // 1. Müşteri Attack animasyonu başlat
            if (customerSpawner != null)
                customerSpawner.PlayAnimation("Attack");

            // 2. Attack animasyonunun vuruş anına kadar bekle
            yield return new WaitForSeconds(AnimationTimings.ATTACK_HIT_DELAY);

            int actionId = Random.Range(0, 4);
            switch (actionId)
            {
                case 0:
                    playerHP -= 25f * customerDebuffMultiplier;
                    if (waiterAnimator != null) waiterAnimator.SetTrigger("Hit");
                    AudioManager.Instance.PlayComplaints();
                    Debug.Log("Musteri: sikayet firtinasi! 25 hasar!");
                    break;
                case 1:
                    playerStunTurns += 1;
                    playerStunImage.gameObject.SetActive(true);
                    playerHP -= 15f * customerDebuffMultiplier;
                    if (waiterAnimator != null) waiterAnimator.SetTrigger("Hit");
                    AudioManager.Instance.PlayAllergy();
                    Debug.Log("Musteri: Alerji tuzagi! Stun +15 hasar!");
                    UpdateTimeline(); // [NEW] Fallback stun
                    break;
                case 2:
                    playerPoisonStacks += 2;
                    AudioManager.Instance.PlayBadComments();
                    playerPoisonImage.gameObject.SetActive(true);
                    Debug.Log("Musteri: Kotu yorum zehri! 2 tur poison!");
                    break;
                case 3:
                    playerDebuffMultiplier = 0.6f;
                    playerDebuffImage.gameObject.SetActive(true);
                    playerDebuffTurns = 2;
                    AudioManager.Instance.PlayShock();
                    Debug.Log("Musteri: Hesap soku! Debuff %40!");
                    break;
            }
        }

        // 5. Hit animasyonu bitene kadar bekle
        yield return new WaitForSeconds(AnimationTimings.HIT_REACTION);

        UpdateHPUI();
        
        // 6. Win/Lose kontrolü - eğer oyun bittiyse FinishCustomerTurn çağrılmaz
        bool gameEnded = CheckWinLoseAndReturn();
        
        isAnimationPlaying = false;

        // Extra turn aksiyonu değilse ve oyun bitmediyse FinishCustomerTurn çağır
        if (!gameEnded && !isExtraTurnAction)
        {
            FinishCustomerTurn();
        }
    }


    void UpdateHPUI()
    {
        if (playerHP < 0) playerHP = 0;
        if (customerHP < 0) customerHP = 0;
        
        playerHPSlider.value = playerHP;
        customerHPSlider.value = customerHP;

        playerHPText.text = playerHP.ToString();
        customerHPText.text = customerHP.ToString();
    }

    void UpdateStaminaUI()
    {
        if (staminaText != null)
            staminaText.text = $"{playerStamina}/{maxStamina}";
        
        if (staminaSlider != null)
            staminaSlider.value = playerStamina;
    }

    void UpdateTurnText()
    {
        if (turnText != null)
            turnText.text = isPlayerTurn ? "YOUR TURN" : "ENEMY TURN";
    }

    void UpdateTimeline()
    {
        if (turnTimeline != null)
        {
            turnTimeline.UpdateTimeline(isPlayerTurn, playerStunTurns, customerStunTurns);
        }
    }

    void CheckWinLose()
    {
        if (customerHP <= 0)
        {
            lastMatchWon = true;
            winLosePanel.SetActive(true);
            
            // Coin kazan (kalan HP = bahşiş)
            int coinsEarned = (int)playerHP;
            SaveSystem.AddCoins(coinsEarned);
            
            // Son müşteri mi kontrol et
            bool isLastCustomer = customerSpawner != null && 
                currentCustomerIndex >= customerSpawner.availableCustomers.Length - 1;
            
            // Level ilerlemesi
            if (!isLastCustomer)
            {
                // Sonraki level'i aç
                int nextLevel = currentCustomerIndex + 2; // 0-indexed + 1 for next + 1 for 1-based
                if (nextLevel > SaveSystem.CurrentLevel)
                    SaveSystem.CurrentLevel = nextLevel;
            }
            
            if (isLastCustomer)
                winLoseText.text = "CONGRATULATIONS!\nYou defeated all customers!";
            else
                winLoseText.text = "Victory!";
            winCoinText.text = $"+${coinsEarned} tip!";

            // Buton metnini güncelle - her zaman ana menüye dön
            if (continueButtonText != null)
                continueButtonText.text = "MAIN MENU";
            
            AudioManager.Instance.PlayWin();
            isPlayerTurn = false;
            
            // Müşteri yenildi animasyonu
            if (customerSpawner != null)
                customerSpawner.OnDefeat();
            
            // AI öğrenme: Oyuncu kazandı (AI kaybetti)
            if (customerAI != null)
                customerAI.OnEpisodeEnd(false);

            if(waiterAnimator != null)
                waiterAnimator.SetTrigger("Victory");
        }
        else if (playerHP <= 0)
        {
            lastMatchWon = false;
            winLosePanel.SetActive(true);
            winLoseText.text = "Customer wins!";
            winCoinText.text = "$0 tip";

            // Buton metnini güncelle - her zaman ana menüye dön
            if (continueButtonText != null)
                continueButtonText.text = "MAIN MENU";
            
            AudioManager.Instance.PlayLose();
            isPlayerTurn = false;
            
            // Müşteri kazandı animasyonu
            if (customerSpawner != null)
                customerSpawner.PlayAnimation("Victory");
            
            // AI öğrenme: AI kazandı
            if (customerAI != null)
                customerAI.OnEpisodeEnd(true);

            if(waiterAnimator != null) 
                waiterAnimator.SetTrigger("Defeat");
        }
    }

    /// <summary>
    /// Win/Lose kontrolü - oyun bitip bitmediğini döndürür
    /// </summary>
    bool CheckWinLoseAndReturn()
    {
        if (customerHP <= 0 || playerHP <= 0)
        {
            CheckWinLose();
            return true;
        }
        return false;
    }

    // Stamina bilgilerini dışarı expose et (UI için)
    public int GetPlayerStamina() => playerStamina;
    public int GetMaxStamina() => maxStamina;

    // Kart stamina kontrolü için (CardDisplay'de kullanılacak)
    public bool CanAffordCard(CardData card)
    {
        return playerStamina >= card.staminaCost;
    }

    /// <summary>
    /// Continue butonu - her zaman ana menüye dön
    /// </summary>
    public void OnContinueButtonClicked()
    {
        if (lastMatchWon)
        {
            // Kazandı - Level ilerlemesi zaten CheckWinLose'da yapıldı
            currentCustomerIndex++;
        }
        // Kaybetti veya kazandı - her zaman ana menüye dön
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Yeni maç başlatır - tüm değerleri sıfırlar
    /// </summary>
    private void StartNewMatch()
    {
        // HP sıfırla
        playerHP = maxHP;

        // Stamina sıfırla
        playerStamina = maxStamina;
        UpdateStaminaUI();
        
        // Status efektleri sıfırla
        playerPoisonStacks = 0;
        playerStunTurns = 0;
        playerDebuffTurns = 0;
        playerDebuffMultiplier = 1f;
        customerPoisonStacks = 0;
        customerStunTurns = 0;
        customerDebuffTurns = 0;
        customerDebuffMultiplier = 1f;
        customerTurnCounter = 0; // Food Blogger için tur sayacı sıfırla
        
        // Status UI'larını gizle
        if (playerStunImage != null) playerStunImage.SetActive(false);
        if (playerDebuffImage != null) playerDebuffImage.SetActive(false);
        if (playerPoisonImage != null) playerPoisonImage.SetActive(false);
        if (customerStunImage != null) customerStunImage.SetActive(false);
        if (customerDebuffImage != null) customerDebuffImage.SetActive(false);
        if (customerPoisonImage != null) customerPoisonImage.SetActive(false);
        
        // Yeni müşteriyi spawn et
        if (customerSpawner != null)
            customerSpawner.SpawnCustomerByIndex(currentCustomerIndex);
        
        // Panel kapat, oyunu başlat
        winLosePanel.SetActive(false);
        isPlayerTurn = true;
        UpdateHPUI();
        UpdateTurnText();
        
        // Animasyonları Idle'a döndür
        if (waiterAnimator != null)
            waiterAnimator.SetTrigger("Idle");

        UpdateTimeline(); // [FIX] isPlayerTurn ayarlandıktan sonra timeline'ı güncelle
    }
}