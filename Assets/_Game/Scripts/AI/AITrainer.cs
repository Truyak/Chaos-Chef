using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Tam otomatik AI eğitim sistemi
/// Sahneyi aç, Play'e bas, eğitim otomatik başlar
/// </summary>
public class AITrainer : MonoBehaviour
{
    [Header("Training Settings")]
    [SerializeField] private bool autoStartTraining = true;
    [SerializeField] private int targetEpisodes = 1000;
    [SerializeField] private int saveInterval = 100; // Her 100 episode'da kaydet

    [Header("References")]
    [SerializeField] private CardData[] playerDeck;
    
    [Header("UI (Optional)")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Slider progressBar;

    // Simülasyon değişkenleri
    private float simPlayerHP;
    private float simCustomerHP;
    private float simMaxHP = 100f;
    
    private int simPlayerPoisonStacks;
    private int simPlayerStunTurns;
    private int simPlayerDebuffTurns;
    private float simPlayerDebuffMultiplier;
    
    private int simCustomerPoisonStacks;
    private int simCustomerStunTurns;
    private int simCustomerDebuffTurns;
    private float simCustomerDebuffMultiplier;

    private TrainingPlayer trainingPlayer;
    private CustomerAI customerAI;
    
    private int currentEpisode = 0;
    private int playerWins = 0;
    private int customerWins = 0;
    private bool isTraining = false;
    
    // Ek istatistikler
    private float trainingStartTime;
    private int totalTurns = 0;
    private int aiStunCount = 0;
    private int aiPoisonCount = 0;
    private int aiHighDamageCount = 0;

    private void Start()
    {
        customerAI = CustomerAI.Instance;
        
        if (customerAI == null)
        {
            // CustomerAI yoksa oluştur
            GameObject aiObj = new GameObject("CustomerAI");
            customerAI = aiObj.AddComponent<CustomerAI>();
        }

        trainingPlayer = gameObject.AddComponent<TrainingPlayer>();
        trainingPlayer.Initialize(playerDeck);

        // Önceki eğitimi yükle
        customerAI.LoadQTable();
        currentEpisode = customerAI.GetEpisodeCount();

        if (autoStartTraining && currentEpisode < targetEpisodes)
        {
            StartTraining();
        }
        else if (currentEpisode >= targetEpisodes)
        {
            UpdateStatus($"Eğitim tamamlandı! {currentEpisode} episode.");
        }
    }

    public void StartTraining()
    {
        if (isTraining) return;
        
        isTraining = true;
        customerAI.SetTrainingMode(true);
        trainingStartTime = Time.realtimeSinceStartup;
        totalTurns = 0;
        aiStunCount = 0;
        aiPoisonCount = 0;
        aiHighDamageCount = 0;
        
        StartCoroutine(TrainingLoop());
    }

    public void StopTraining()
    {
        isTraining = false;
        customerAI.SetTrainingMode(false);
        customerAI.SaveQTable();
    }

    private IEnumerator TrainingLoop()
    {
        UpdateStatus("Eğitim başlıyor...");
        
        while (isTraining && currentEpisode < targetEpisodes)
        {
            // Yeni episode başlat
            RunEpisode();
            currentEpisode++;

            // Progress güncelle
            if (progressBar != null)
                progressBar.value = (float)currentEpisode / targetEpisodes;

            // Periyodik kayıt
            if (currentEpisode % saveInterval == 0)
            {
                customerAI.SaveQTable();
                float winRate = (float)customerWins / (playerWins + customerWins) * 100f;
                UpdateStatus($"Episode: {currentEpisode}/{targetEpisodes} | AI Win Rate: {winRate:F1}% | States: {customerAI.GetQTableSize()}");
            }

            // Her 10 episode'da bir frame bekle (UI güncelleme için)
            if (currentEpisode % 10 == 0)
            {
                yield return null;
            }
        }

        // Eğitim tamamlandı
        isTraining = false;
        customerAI.SetTrainingMode(false);
        customerAI.SaveQTable();
        
        PrintFinalStatistics();
    }

    private void RunEpisode()
    {
        // Reset game state
        simPlayerHP = simMaxHP;
        simCustomerHP = simMaxHP;
        
        simPlayerPoisonStacks = 0;
        simPlayerStunTurns = 0;
        simPlayerDebuffTurns = 0;
        simPlayerDebuffMultiplier = 1f;
        
        simCustomerPoisonStacks = 0;
        simCustomerStunTurns = 0;
        simCustomerDebuffTurns = 0;
        simCustomerDebuffMultiplier = 1f;

        trainingPlayer.ResetForNewEpisode();

        bool gameOver = false;
        bool isPlayerTurn = true;
        int turnCount = 0;
        int maxTurns = 100; // Sonsuz döngü koruması

        while (!gameOver && turnCount < maxTurns)
        {
            turnCount++;

            if (isPlayerTurn)
            {
                // Oyuncu turu
                SimulatePlayerTurn();
                
                if (simCustomerHP <= 0)
                {
                    gameOver = true;
                    playerWins++;
                    customerAI.OnEpisodeEnd(false);
                }
            }
            else
            {
                // Müşteri (AI) turu
                SimulateCustomerTurn();
                
                if (simPlayerHP <= 0)
                {
                    gameOver = true;
                    customerWins++;
                    customerAI.OnEpisodeEnd(true);
                }
            }

            isPlayerTurn = !isPlayerTurn;
        }

        // Max tur aşıldıysa berabere say (AI kaybetmiş gibi)
        if (!gameOver)
        {
            customerAI.OnEpisodeEnd(false);
        }
    }

    private void SimulatePlayerTurn()
    {
        // Tur başı efektleri
        if (simPlayerPoisonStacks > 0)
        {
            simPlayerHP -= 10f;
            simPlayerPoisonStacks--;
        }

        if (simPlayerDebuffTurns > 0)
        {
            simPlayerDebuffTurns--;
            if (simPlayerDebuffTurns <= 0)
                simPlayerDebuffMultiplier = 1f;
        }

        // Stun kontrolü
        if (simPlayerStunTurns > 0)
        {
            simPlayerStunTurns--;
            return; // Tur geç
        }

        trainingPlayer.OnTurnStart();

        // Kart seç ve oyna
        CardData card = trainingPlayer.SelectCard(
            simPlayerHP, simCustomerHP, simMaxHP,
            simCustomerStunTurns, simCustomerPoisonStacks
        );

        if (card != null)
        {
            // Gerçek kart kullan
            float damage = card.baseDamage * simCustomerDebuffMultiplier;
            
            if (card.isHeal)
            {
                simPlayerHP = Mathf.Min(simPlayerHP + card.baseDamage, simMaxHP);
            }
            else
            {
                simCustomerHP -= damage;
            }

            switch (card.effectType)
            {
                case "Poison":
                    simCustomerPoisonStacks += card.duration;
                    break;
                case "Stun":
                    simCustomerStunTurns += card.duration;
                    break;
                case "Debuff":
                    simCustomerDebuffMultiplier = 0.4f;
                    simCustomerDebuffTurns = card.duration;
                    break;
            }
        }
        else
        {
            // Simüle kart kullan (gerçek kart yoksa)
            var (damage, effect, duration) = trainingPlayer.GetSimulatedAction(
                simCustomerHP, simMaxHP,
                simCustomerStunTurns, simCustomerPoisonStacks
            );

            simCustomerHP -= damage * simCustomerDebuffMultiplier;

            switch (effect)
            {
                case "Poison":
                    simCustomerPoisonStacks += duration;
                    break;
                case "Stun":
                    simCustomerStunTurns += duration;
                    break;
                case "Debuff":
                    simCustomerDebuffMultiplier = 0.4f;
                    simCustomerDebuffTurns = duration;
                    break;
            }
        }
    }

    private void SimulateCustomerTurn()
    {
        // Tur başı efektleri
        if (simCustomerPoisonStacks > 0)
        {
            simCustomerHP -= 10f;
            simCustomerPoisonStacks--;
        }

        if (simCustomerDebuffTurns > 0)
        {
            simCustomerDebuffTurns--;
            if (simCustomerDebuffTurns <= 0)
                simCustomerDebuffMultiplier = 1f;
        }

        // Stun kontrolü
        if (simCustomerStunTurns > 0)
        {
            simCustomerStunTurns--;
            customerAI.TickAllCooldowns();
            return;
        }

        // State hesapla
        int state = customerAI.GetStateHash(
            simPlayerHP, simCustomerHP, simMaxHP,
            simPlayerStunTurns, simPlayerPoisonStacks, simPlayerDebuffTurns
        );

        // Aksiyon seç
        int actionIndex = customerAI.SelectAction(state);
        AIAction action = customerAI.ExecuteAction(actionIndex);

        // Aksiyonu uygula
        float damage = action.damage * simPlayerDebuffMultiplier;
        simPlayerHP -= damage;
        totalTurns++;

        // İstatistik takibi
        if (action.effectType == "Stun") aiStunCount++;
        if (action.effectType == "Poison") aiPoisonCount++;
        if (damage >= 25f) aiHighDamageCount++;

        switch (action.effectType)
        {
            case "Stun":
                simPlayerStunTurns += action.effectDuration;
                break;
            case "Poison":
                simPlayerPoisonStacks += action.effectDuration;
                break;
            case "Debuff":
                simPlayerDebuffMultiplier = 0.6f;
                simPlayerDebuffTurns = action.effectDuration;
                break;
        }

        // Reward hesapla ve Q-Table güncelle
        float reward = customerAI.CalculateReward(damage, action.effectType);
        
        int newState = customerAI.GetStateHash(
            simPlayerHP, simCustomerHP, simMaxHP,
            simPlayerStunTurns, simPlayerPoisonStacks, simPlayerDebuffTurns
        );
        
        customerAI.UpdateQTable(newState, reward, false);
        customerAI.TickAllCooldowns();
    }

    private void UpdateStatus(string message)
    {
        Debug.Log($"[AITrainer] {message}");
        if (statusText != null)
            statusText.text = message;
    }

    // ============ EDITOR BUTTONS ============
    
    [ContextMenu("Start Training")]
    public void EditorStartTraining()
    {
        StartTraining();
    }

    [ContextMenu("Stop Training")]
    public void EditorStopTraining()
    {
        StopTraining();
    }

    [ContextMenu("Reset Q-Table")]
    public void EditorResetQTable()
    {
        customerAI?.ResetQTable();
        currentEpisode = 0;
        playerWins = 0;
        customerWins = 0;
        UpdateStatus("Q-Table sıfırlandı!");
    }

    private void PrintFinalStatistics()
    {
        float trainingDuration = Time.realtimeSinceStartup - trainingStartTime;
        int totalGames = playerWins + customerWins;
        float aiWinRate = totalGames > 0 ? (float)customerWins / totalGames * 100f : 0f;
        float playerWinRate = totalGames > 0 ? (float)playerWins / totalGames * 100f : 0f;
        float avgTurnsPerGame = totalGames > 0 ? (float)totalTurns / totalGames : 0f;
        float stunRate = totalTurns > 0 ? (float)aiStunCount / totalTurns * 100f : 0f;
        float poisonRate = totalTurns > 0 ? (float)aiPoisonCount / totalTurns * 100f : 0f;
        float highDmgRate = totalTurns > 0 ? (float)aiHighDamageCount / totalTurns * 100f : 0f;

        Debug.Log("\n" +
            "============================================\n" +
            "         EGITIM ISTATISTIKLERI              \n" +
            "============================================\n" +
            $"  Toplam Oyun: {totalGames:N0}\n" +
            $"  Sure: {trainingDuration:F1} saniye\n" +
            "--------------------------------------------\n" +
            $"  [AI] Kazanma: {aiWinRate:F1}% ({customerWins:N0} win)\n" +
            $"  [PL] Kazanma: {playerWinRate:F1}% ({playerWins:N0} win)\n" +
            "--------------------------------------------\n" +
            $"  Q-Table States: {customerAI.GetQTableSize()}\n" +
            $"  Exploration Rate: {customerAI.GetExplorationRate():F2}\n" +
            "--------------------------------------------\n" +
            $"  Ort. Tur/Oyun: {avgTurnsPerGame:F1}\n" +
            $"  Stun Kullanimi: {stunRate:F1}%\n" +
            $"  Poison Kullanimi: {poisonRate:F1}%\n" +
            $"  Yuksek Hasar (25+): {highDmgRate:F1}%\n" +
            "============================================\n"
        );

        UpdateStatus($"Eğitim tamamlandı! | AI: {aiWinRate:F1}% | Oyuncu: {playerWinRate:F1}% | States: {customerAI.GetQTableSize()}");
    }
}
