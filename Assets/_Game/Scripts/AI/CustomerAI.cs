using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Q-Learning tabanlı müşteri AI sistemi
/// State-Action-Reward döngüsü ile öğrenen yapay zeka
/// </summary>
public class CustomerAI : MonoBehaviour
{
    public static CustomerAI Instance;

    [Header("Pretrained Model")]
    [SerializeField] private TextAsset pretrainedModelFile;

    [Header("Q-Learning Parameters")]
    [SerializeField] private float learningRate = 0.1f;      // α
    [SerializeField] private float discountFactor = 0.95f;   // γ
    [SerializeField] private float explorationRate = 0.3f;   // ε
    [SerializeField] private float minExplorationRate = 0.05f;
    [SerializeField] private float explorationDecay = 0.995f;

    [Header("Training")]
    [SerializeField] private bool isTraining = false;
    [SerializeField] private int episodeCount = 0;

    // Q-Table: State hash -> Action values array
    private Dictionary<int, float[]> qTable = new Dictionary<int, float[]>();

    [System.Serializable]
    public class AIModelData
    {
        public int episodeCount;
        public float explorationRate;
        public List<QTableEntry> qTableEntries = new List<QTableEntry>();
    }

    [System.Serializable]
    public class QTableEntry
    {
        public int state;
        public float[] values;
    }
    
    // AI Actions with cooldowns
    private AIAction[] actions;
    
    private int lastState = -1;
    private int lastAction = -1;
    private int prevAction = -1; // Bir önceki turdaki aksiyon (tekrar kontrolü için)

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        InitializeActions();
    }

    private void InitializeActions()
    {
        // 8 farklı aksiyon - restoran temalı
        actions = new AIAction[]
        {
            new AIAction("Şikayet Fırtınası", 25f, "Damage", 0, 1),        // 0: Yüksek hasar, 1 tur cooldown
            new AIAction("Küçük Şikayet", 12f, "Damage", 0, 1),            // 1: Düşük hasar, 1 tur cooldown
            new AIAction("Alerji Tuzağı", 15f, "Stun", 1, 3),            // 2: Stun + hasar, 3 tur cooldown
            new AIAction("Kötü Yorum Tehdidi", 5f, "Poison", 2, 1),      // 3: Poison, 1 tur cooldown
            new AIAction("Hesap Şoku", 0f, "Debuff", 2, 2),              // 4: Debuff, 2 tur cooldown
            new AIAction("Sosyal Medya Tehdidi", 20f, "Poison", 1, 1),   // 5: Hasar + kısa poison
            new AIAction("Yöneticiyi Çağırma", 30f, "Damage", 0, 2),       // 6: Çok yüksek hasar, 2 tur cooldown
            new AIAction("Pasif Agresif Bakış", 8f, "Debuff", 1, 1),     // 7: Düşük hasar + kısa debuff
        };
    }

    /// <summary>
    /// Mevcut oyun durumunu 7-bit integer olarak encode eder
    /// </summary>
    public int GetStateHash(float playerHP, float customerHP, float maxHP, 
                            int playerStunTurns, int playerPoisonStacks, int playerDebuffTurns)
    {
        int state = 0;
        
        // Player HP: 4 seviye (0-25%, 25-50%, 50-75%, 75-100%)
        float playerHPRatio = playerHP / maxHP;
        int playerHPState = Mathf.Clamp((int)(playerHPRatio * 4), 0, 3);
        state |= playerHPState;
        
        // Customer HP: 4 seviye
        float customerHPRatio = customerHP / maxHP;
        int customerHPState = Mathf.Clamp((int)(customerHPRatio * 4), 0, 3);
        state |= (customerHPState << 2);
        
        // Status effects
        state |= (playerStunTurns > 0 ? 1 : 0) << 4;
        state |= (playerPoisonStacks > 0 ? 1 : 0) << 5;
        state |= (playerDebuffTurns > 0 ? 1 : 0) << 6;
        
        return state; // 0-127 arası değer
    }

    /// <summary>
    /// Mevcut duruma göre en iyi aksiyonu seçer (ε-greedy policy)
    /// </summary>
    public int SelectAction(int state)
    {
        // Q-Table'da bu state yoksa oluştur
        if (!qTable.ContainsKey(state))
        {
            qTable[state] = new float[actions.Length];
        }

        // Kullanılabilir aksiyonları bul
        List<int> availableActions = new List<int>();
        for (int i = 0; i < actions.Length; i++)
        {
            if (actions[i].IsAvailable())
                availableActions.Add(i);
        }

        // Hiç kullanılabilir aksiyon yoksa tüm cooldown'ları sıfırla (edge case)
        if (availableActions.Count == 0)
        {
            foreach (var action in actions)
                action.Reset();
            availableActions = Enumerable.Range(0, actions.Length).ToList();
        }

        int selectedAction;

        // Exploration vs Exploitation
        if (isTraining && Random.value < explorationRate)
        {
            // Rastgele keşif (sadece kullanılabilir aksiyonlardan)
            selectedAction = availableActions[Random.Range(0, availableActions.Count)];
        }
        else
        {
            // En yüksek Q-değerine sahip aksiyonu seç (sadece kullanılabilirlerden)
            float maxQ = float.MinValue;
            selectedAction = availableActions[0];
            
            foreach (int actionIdx in availableActions)
            {
                if (qTable[state][actionIdx] > maxQ)
                {
                    maxQ = qTable[state][actionIdx];
                    selectedAction = actionIdx;
                }
            }
        }

        lastState = state;
        prevAction = lastAction; // Önceki aksiyonu kaydet
        lastAction = selectedAction;
        
        return selectedAction;
    }

    /// <summary>
    /// Seçilen aksiyonu uygular ve bilgilerini döndürür
    /// </summary>
    public AIAction ExecuteAction(int actionIndex)
    {
        if (actionIndex < 0 || actionIndex >= actions.Length)
            actionIndex = 0;

        AIAction action = actions[actionIndex];
        action.Use(); // Cooldown başlat
        
        return action;
    }

    /// <summary>
    /// Tüm aksiyonların cooldown'larını 1 azaltır (tur sonu)
    /// </summary>
    public void TickAllCooldowns()
    {
        foreach (var action in actions)
        {
            action.TickCooldown();
        }
    }

    /// <summary>
    /// Q-Table'ı günceller (reward feedback)
    /// </summary>
    public void UpdateQTable(int newState, float reward, bool isTerminal)
    {
        if (lastState < 0 || lastAction < 0) return;

        if (!qTable.ContainsKey(lastState))
            qTable[lastState] = new float[actions.Length];
        
        if (!qTable.ContainsKey(newState))
            qTable[newState] = new float[actions.Length];

        float oldQ = qTable[lastState][lastAction];
        float maxNextQ = isTerminal ? 0f : qTable[newState].Max();
        
        // Q-Learning update formula
        float newQ = oldQ + learningRate * (reward + discountFactor * maxNextQ - oldQ);
        qTable[lastState][lastAction] = newQ;
    }

    /// <summary>
    /// Episode sonunda çağrılır
    /// </summary>
    public void OnEpisodeEnd(bool customerWon)
    {
        float finalReward = customerWon ? 100f : -100f;
        UpdateQTable(0, finalReward, true);
        
        episodeCount++;
        
        // Exploration rate decay
        if (explorationRate > minExplorationRate)
        {
            explorationRate *= explorationDecay;
        }
        
        // Cooldown'ları sıfırla
        foreach (var action in actions)
            action.Reset();
        
        lastState = -1;
        lastAction = -1;
        prevAction = -1;
    }

    /// <summary>
    /// Reward hesaplama (her aksiyon sonrası)
    /// </summary>
    public float CalculateReward(float damageDealt, string effectApplied)
    {
        float reward = damageDealt / 10f; // Hasar için küçük reward
        
        switch (effectApplied)
        {
            case "Stun":
                reward += 25f; // Stun çok değerli
                break;
            case "Poison":
                reward += 15f; // DoT etkili
                break;
            case "Debuff":
                reward += 12f; // Hasar azaltma
                break;
        }

        // Tekrar Cezası (Repetition Penalty)
        // Eğer aynı aksiyonu üst üste yaparsa ciddi ceza ver
        if (lastAction == prevAction && lastAction != -1)
        {
            reward -= 15f; // Spam yapmayı engelle
        }
        
        return reward;
    }

    // ============ SAVE / LOAD ============

    public void SetTrainingMode(bool training)
    {
        isTraining = training;
        if (training)
        {
            explorationRate = 0.5f; // Training başlangıcında yüksek keşif
        }
        else
        {
            explorationRate = minExplorationRate; // Normal oyunda düşük keşif
        }
    }

    public int GetEpisodeCount() => episodeCount;
    public int GetQTableSize() => qTable.Count;
    public float GetExplorationRate() => explorationRate;

#if UNITY_EDITOR
    [ContextMenu("Export Q-Table to JSON")]
    public void ExportModelToJson()
    {
        AIModelData data = new AIModelData();
        data.episodeCount = episodeCount;
        data.explorationRate = explorationRate;
        
        foreach (var kvp in qTable)
        {
            QTableEntry entry = new QTableEntry();
            entry.state = kvp.Key;
            entry.values = kvp.Value;
            data.qTableEntries.Add(entry);
        }

        string json = JsonUtility.ToJson(data, true);
        string path = "Assets/_Game/Resources/AI/CustomerAI_Model.json";
        
        System.IO.File.WriteAllText(path, json);
        UnityEditor.AssetDatabase.Refresh();
        Debug.Log($"[CustomerAI] Model exported to {path}");
    }
#endif

    public string SerializeQTable()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var kvp in qTable)
        {
            sb.Append(kvp.Key);
            sb.Append(":");
            sb.Append(string.Join(",", kvp.Value));
            sb.Append(";");
        }
        return sb.ToString();
    }

    public void DeserializeQTable(string data)
    {
        if (string.IsNullOrEmpty(data)) return;
        
        qTable.Clear();
        string[] entries = data.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
        
        foreach (string entry in entries)
        {
            string[] parts = entry.Split(':');
            if (parts.Length == 2)
            {
                int state = int.Parse(parts[0]);
                string[] values = parts[1].Split(',');
                float[] qValues = new float[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    float.TryParse(values[i], out qValues[i]);
                }
                qTable[state] = qValues;
            }
        }
    }

    public void SaveQTable()
    {
        string data = SerializeQTable();
        PlayerPrefs.SetString("CustomerAI_QTable", data);
        PlayerPrefs.SetInt("CustomerAI_Episodes", episodeCount);
        PlayerPrefs.SetFloat("CustomerAI_ExplorationRate", explorationRate);
        PlayerPrefs.Save();
        Debug.Log($"[CustomerAI] Q-Table saved! States: {qTable.Count}, Episodes: {episodeCount}");
    }

    public void LoadQTable()
    {
        // 1. Önce PlayerPrefs kontrol et (sürdürülen eğitim/oyun)
        if (PlayerPrefs.HasKey("CustomerAI_QTable"))
        {
            string data = PlayerPrefs.GetString("CustomerAI_QTable");
            DeserializeQTable(data);
            episodeCount = PlayerPrefs.GetInt("CustomerAI_Episodes", 0);
            explorationRate = PlayerPrefs.GetFloat("CustomerAI_ExplorationRate", 0.3f);
            Debug.Log($"[CustomerAI] Q-Table loaded from PlayerPrefs! States: {qTable.Count}, Episodes: {episodeCount}");
        }
        // 2. Yoksa Pretrained model yükle
        else if (pretrainedModelFile != null)
        {
            LoadPretrainedModel();
        }
    }

    public void LoadPretrainedModel()
    {
        if (pretrainedModelFile == null)
        {
            Debug.LogWarning("[CustomerAI] Pretrained model file is missing!");
            return;
        }

        AIModelData data = JsonUtility.FromJson<AIModelData>(pretrainedModelFile.text);
        if (data != null)
        {
            qTable.Clear();
            episodeCount = data.episodeCount;
            // Exploration rate'i modelden alabiliriz ama oyunda genelde düşük başlarız
            // Yine de kaydettiğimiz eğitim durumundan devam etmek için alalım
            explorationRate = data.explorationRate; 
            
            foreach (var entry in data.qTableEntries)
            {
                qTable[entry.state] = entry.values;
            }
            
            Debug.Log($"[CustomerAI] Q-Table loaded from Pretrained JSON! States: {qTable.Count}, Episodes: {episodeCount}");
        }
    }

    public void ResetQTable()
    {
        qTable.Clear();
        episodeCount = 0;
        explorationRate = 0.3f;
        PlayerPrefs.DeleteKey("CustomerAI_QTable");
        PlayerPrefs.DeleteKey("CustomerAI_Episodes");
        PlayerPrefs.DeleteKey("CustomerAI_ExplorationRate");
        Debug.Log("[CustomerAI] Q-Table reset!");
    }
}
