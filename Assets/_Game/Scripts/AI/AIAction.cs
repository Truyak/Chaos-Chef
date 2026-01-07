using UnityEngine;

/// <summary>
/// AI'ın kullanabileceği aksiyonları tanımlar
/// </summary>
[System.Serializable]
public class AIAction
{
    public string actionName;
    public float damage;
    public string effectType; // "None", "Stun", "Poison", "Debuff"
    public int effectDuration;
    public int cooldownTurns; // Kullanım sonrası bekleme süresi
    
    [HideInInspector]
    public int currentCooldown = 0; // Şu anki cooldown sayacı

    public AIAction(string name, float dmg, string effect, int duration, int cooldown)
    {
        actionName = name;
        damage = dmg;
        effectType = effect;
        effectDuration = duration;
        cooldownTurns = cooldown;
        currentCooldown = 0;
    }

    public bool IsAvailable()
    {
        return currentCooldown <= 0;
    }

    public void Use()
    {
        currentCooldown = cooldownTurns;
    }

    public void TickCooldown()
    {
        if (currentCooldown > 0)
            currentCooldown--;
    }

    public void Reset()
    {
        currentCooldown = 0;
    }
}
