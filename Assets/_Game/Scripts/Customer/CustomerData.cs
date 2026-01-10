using UnityEngine;

/// <summary>
/// Her müşteri tipinin verilerini tutan ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "NewCustomer", menuName = "Chaos Chef/Customer Data")]
public class CustomerData : ScriptableObject
{
    [Header("Temel Bilgiler")]
    public string customerName = "Müşteri";
    public CustomerType customerType = CustomerType.Nervous;
    
    [Header("Görsel")]
    public GameObject modelPrefab;
    public RuntimeAnimatorController animatorController;
    public Sprite icon; // [NEW] Müşteri ikonu (Timeline için)
    
    [Header("Stats")]
    public float maxHP = 100f;
    public float damageMultiplier = 1f; // AI hasarı çarpanı
    
    [Header("AI Davranışı")]
    public AIBehaviorProfile behaviorProfile = AIBehaviorProfile.Balanced;
    [Range(0f, 1f)]
    public float stunChance = 0.2f;      // Stun kullanma olasılığı
    [Range(0f, 1f)]
    public float poisonChance = 0.2f;    // Poison kullanma olasılığı
    [Range(0f, 1f)]
    public float debuffChance = 0.2f;    // Debuff kullanma olasılığı
    
    [Header("Özel Yetenekler")]
    public bool hasExtraTurn = false;     // Food Blogger için
    public int extraTurnInterval = 3;     // Kaç turda bir ekstra tur
    
    [Header("Açıklama")]
    [TextArea(2, 4)]
    public string description = "Müşteri açıklaması...";
}

/// <summary>
/// Müşteri tipleri enum
/// </summary>
public enum CustomerType
{
    Nervous,    // Acemi Müşteri
    Impatient,  // Sabırsız Müşteri
    Critic,     // Yelp Eleştirmeni
    Blogger,    // Food Blogger
    Boss        // Restoran Sahibi
}

/// <summary>
/// AI davranış profilleri
/// </summary>
public enum AIBehaviorProfile
{
    Passive,    // Düşük hasar, çok efekt
    Aggressive, // Yüksek hasar, az efekt
    Balanced,   // Dengeli
    Strategic,  // Efekt odaklı (DoT)
    AllRounder  // Tüm yetenekler (Boss)
}
