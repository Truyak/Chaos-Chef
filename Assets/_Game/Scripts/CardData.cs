using UnityEngine;

[CreateAssetMenu]
public class CardData : ScriptableObject
{
    public string cardName;
    public Sprite artwork;
    public float baseDamage = 30f;
    public string effectType; // "Damage", "Stun", "Poison", "Debuff"
    public int duration;
    public string description;
}