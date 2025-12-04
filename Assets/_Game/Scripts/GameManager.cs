using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
    public GameObject winLosePanel;
    public TextMeshProUGUI turnText;
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


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        winLosePanel.SetActive(false);
    }

    private void Start()
    {
        playerHPSlider.maxValue = maxHP;
        customerHPSlider.maxValue = maxHP;
        UpdateHPUI();
        winLosePanel.SetActive(false);
        AITurnButton.interactable = false;

        SetupImages();
        SpawnDeck();
        UpdateTurnText();
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
        startingDeck = ShuffleArray(startingDeck);
        drawPile.Clear();

        for (int i = 0; i < 4; i++)
        {
            if (i < startingDeck.Length)
            {
                GameObject newCard = Instantiate(cardPrefab, deckPanel);
                newCard.GetComponent<CardDisplay>().Setup(startingDeck[i]);
            }
        }

        for (int i = 4; i < startingDeck.Length; i++)
        {
            drawPile.Enqueue(startingDeck[i]);
        }


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
        if (!isPlayerTurn || Time.timeScale == 0f) return;

        CardData card = playedCardDisplay.cardData;

        float damage = card.baseDamage * customerDebuffMultiplier;
        customerHP -= damage;
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
        }

        drawPile.Enqueue(card);
        CardData nextCard = drawPile.Dequeue();

        playedCardDisplay.Setup(nextCard);

        UpdateHPUI();
        CheckWinLose();

        AIPlayerTurn();
    }

    public void AIPlayerTurn()
    {
        isPlayerTurn = false;
        AITurnButton.interactable = false;

        HandleTurnStart(false);
    }

    public void FinishCustomerTurn()
    {
        isPlayerTurn = true;
        AITurnButton.interactable = false;

        HandleTurnStart(true);
    }

    void HandleTurnStart(bool isPlayerNext)
    {
        UpdateTurnText();
        if (Time.timeScale == 0f) return;

        if (isPlayerNext)
        {
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

            if (customerHP > 0)
            {
                isPlayerTurn = false;
                AITurnButton.interactable = true;
            }
        }

        UpdateHPUI();
        CheckWinLose();
    }

    public void CustomerAction()
    {
        int actionId = Random.Range(0, 4);
        switch (actionId)
        {
            case 0:
                playerHP -= 25f * playerDebuffMultiplier;
                AudioManager.Instance.PlayComplaints();
                Debug.Log("Musteri: sikayet firtinasi! 25 hasar!");
                break;
            case 1:
                playerStunTurns += 1;
                playerStunImage.gameObject.SetActive(true);
                playerHP -= 15f * playerDebuffMultiplier;
                AudioManager.Instance.PlayAllergy();
                Debug.Log("Musteri: Alerji tuzagi! Stun +15 hasar!");
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

        UpdateHPUI();
        CheckWinLose();

        FinishCustomerTurn();
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
    void UpdateTurnText()
    {
        if (turnText != null)
            turnText.text = isPlayerTurn ? "OYUNCU SIRASI" : "MUSTERI SIRASI";
    }

    void CheckWinLose()
    {
        if (customerHP <= 0)
        {
            winLosePanel.SetActive(true);
            winLoseText.text = $"Tebrikler!\n${(int)playerHP} bahsis kaptin!";
            AudioManager.Instance.PlayWin();
            isPlayerTurn = false;
        }
        else if (playerHP <= 0)
        {
            winLosePanel.SetActive(true);
            winLoseText.text = "0$ bahsis...\nMusteri kazandi!";
            AudioManager.Instance.PlayLose();
            isPlayerTurn = false;
        }
    }
}