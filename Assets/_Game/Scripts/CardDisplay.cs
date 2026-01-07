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

    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;

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
        UpdateAffordability();
    }

    private void Update()
    {
        // Her tur başında stamina değişebilir, görsel güncelle
        UpdateAffordability();
    }

    private void UpdateAffordability()
    {
        if (cardData == null) return;
        
        bool canAfford = GameManager.Instance.CanAffordCard(cardData);
        
        // Yetersiz stamina = karartma efekti
        if (canvasGroup != null)
        {
            canvasGroup.alpha = canAfford ? 1f : 0.5f;
        }
    }

    // ------------------- DRAG & DROP -------------------
    public void OnBeginDrag(PointerEventData eventData)
    {
        if(!GameManager.Instance.isPlayerTurn || Time.timeScale == 0f) return;
        
        // Stamina kontrolü - sürükleme başlamadan önce
        if (!GameManager.Instance.CanAffordCard(cardData))
        {
            Debug.Log($"Yetersiz stamina! {cardData.cardName} için {cardData.staminaCost} stamina gerekli.");
            return;
        }

        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(canvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(!GameManager.Instance.isPlayerTurn || Time.timeScale == 0f) return;
        
        // Stamina kontrolü
        if (!GameManager.Instance.CanAffordCard(cardData)) return;
        
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!GameManager.Instance.isPlayerTurn || Time.timeScale == 0f) return;
        
        // Stamina kontrolü
        if (!GameManager.Instance.CanAffordCard(cardData))
        {
            UpdateAffordability();
            return;
        }
        
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (eventData.pointerEnter != null && eventData.pointerEnter.CompareTag("PlayZone"))
        {
            GameManager.Instance.PlayCard(this);
        }

        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
        UpdateAffordability();
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