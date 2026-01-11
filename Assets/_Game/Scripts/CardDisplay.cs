using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    public CardData cardData;
    public Image artworkImage;
    public TextMeshProUGUI staminaCostText; // Sol üst köşede stamina maliyeti
    public Image cardBackground; // Karartma için ana kart görseli

    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;
    private bool isDragging = false;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Setup(CardData data)
    {
        cardData = data;
        artworkImage.sprite = data.artwork;
        
        // Stamina maliyeti göster
        if (staminaCostText != null)
            staminaCostText.text = data.staminaCost.ToString();
        
        UpdateAffordability();
    }

    private void Update()
    {
        // Her frame stamina değişebilir, görsel güncelle
        if (!isDragging)
            UpdateAffordability();
    }

    private void UpdateAffordability()
    {
        if (cardData == null || GameManager.Instance == null) return;
        
        bool canAfford = GameManager.Instance.CanAffordCard(cardData);
        
        // Yetersiz stamina = tüm kartı karart
        if (canvasGroup != null)
        {
            canvasGroup.alpha = canAfford ? 1f : 0.4f;
        }
        
        // Ek olarak arkaplan rengini de karartabiliriz
        if (cardBackground != null)
        {
            cardBackground.color = canAfford ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
        }
    }

    // ------------------- DRAG & DROP -------------------
    public void OnBeginDrag(PointerEventData eventData)
    {
        
        if(!GameManager.Instance.isPlayerTurn || Time.timeScale == 0f)
        {
            Debug.Log("[CardDisplay] Oyuncu sırası değil veya oyun durdu.");
            return;
        }
        
        // Stamina kontrolü - sürükleme başlamadan önce
        if (!GameManager.Instance.CanAffordCard(cardData))
        {
            Debug.Log($"[CardDisplay] Yetersiz stamina! {cardData.cardName} için {cardData.staminaCost} gerekli.");
            return;
        }

        isDragging = true;
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(canvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(!isDragging) return;
        if(!GameManager.Instance.isPlayerTurn || Time.timeScale == 0f) return;
        
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
        if (!isDragging) 
        {
            Debug.Log("[CardDisplay] Drag başlamamıştı, return.");
            return;
        }
        
        isDragging = false;
        canvasGroup.blocksRaycasts = true;

        bool cardPlayed = false;
        
        if (eventData.pointerEnter != null)
        {
            
            if (eventData.pointerEnter.CompareTag("PlayZone"))
            {
                Debug.Log("[CardDisplay] PlayZone'a bırakıldı, PlayCard çağrılıyor...");
                GameManager.Instance.PlayCard(this);
                cardPlayed = true;
            }
        }
        else
        {
            Debug.Log("[CardDisplay] pointerEnter NULL!");
        }

        // Kartı eski yerine döndür
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
        
        // Affordability güncelle
        UpdateAffordability();
        
        if (!cardPlayed)
        {
            Debug.Log("[CardDisplay] Kart oynanmadı, eski yerine döndü.");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Time.timeScale == 0f) return;
        rectTransform.localScale = Vector3.one * 1.15f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Time.timeScale == 0f) return;
        rectTransform.localScale = Vector3.one;
    }
}
