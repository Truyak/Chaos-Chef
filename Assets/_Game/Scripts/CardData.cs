using UnityEngine;

[CreateAssetMenu]
public class CardData : ScriptableObject
{
    public string cardName;
    public Sprite artwork;
    public float baseDamage = 30f;
    public string effectType; // "Damage", "Stun", "Poison", "Debuff", "Heal"
    public int duration;
    public string description;
    
    [Header("Stamina System")]
    public int staminaCost = 1; // Stun kartları için 3, Cold Soup için 4
    
    [Header("Special Effects")]
    public bool isHeal = false; // Heal kartları için true
}