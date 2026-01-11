using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public Button playButton;
    public TextMeshProUGUI playButtonText;
    public TextMeshProUGUI coinsText;
    public Button cardsButton;
    public Button resetButton;
    public Button quitButton;
    
    [Header("AI Toggle")]
    public Toggle aiToggle;
    public TextMeshProUGUI aiToggleLabel;
    
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject cardsPanel;
    
    private void Start()
    {
        UpdateUI();
        
        // Event listeners
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonPressed);
        if (cardsButton != null)
            cardsButton.onClick.AddListener(OnCardsButtonPressed);
        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetButtonPressed);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitButtonPressed);
        if (aiToggle != null)
        {
            aiToggle.isOn = SaveSystem.AILoaded;
            aiToggle.onValueChanged.AddListener(OnAIToggleChanged);
        }
    }
    
    private void UpdateUI()
    {
        // Coins display
        if (coinsText != null)
            coinsText.text = $"Coins: {SaveSystem.Coins}";
        
        // Play button - show current level and deck status
        int equippedCount = SaveSystem.GetEquippedCount();
        bool canPlay = equippedCount >= SaveSystem.MIN_EQUIPPED_CARDS;
        
        if (playButtonText != null)
        {
            if (canPlay)
                playButtonText.text = $"PLAY LEVEL {SaveSystem.CurrentLevel}\n({equippedCount}/8 cards)";
            else
                playButtonText.text = $"NEED {SaveSystem.MIN_EQUIPPED_CARDS} CARDS\n({equippedCount}/8)";
        }
        
        if (playButton != null)
            playButton.interactable = canPlay;
        
        // AI toggle label
        if (aiToggleLabel != null)
            aiToggleLabel.text = SaveSystem.AILoaded ? "AI: Smart" : "AI: Random";
    }
    
    public void OnPlayButtonPressed()
    {
        // 8 kart kontrolü (0 = default deck kullan)
        int equippedCount = SaveSystem.GetEquippedCount();
        if (equippedCount > 0 && equippedCount < SaveSystem.MIN_EQUIPPED_CARDS)
        {
            Debug.Log($"[MainMenu] Need at least {SaveSystem.MIN_EQUIPPED_CARDS} equipped cards!");
            return;
        }
        
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
    
    public void OnCardsButtonPressed()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (cardsPanel != null) cardsPanel.SetActive(true);
    }
    
    public void OnBackToMainMenu()
    {
        if (cardsPanel != null) cardsPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
        UpdateUI();
    }
    
    public void OnAIToggleChanged(bool isOn)
    {
        SaveSystem.AILoaded = isOn;
        UpdateUI();
        Debug.Log($"[MainMenu] AI Mode: {(isOn ? "Smart" : "Random")}");
    }
    
    public void OnResetButtonPressed()
    {
        SaveSystem.ResetAll();
        UpdateUI();
        Debug.Log("[MainMenu] All progress reset!");
    }
    
    public void OnQuitButtonPressed()
    {
        Application.Quit();
    }
}
