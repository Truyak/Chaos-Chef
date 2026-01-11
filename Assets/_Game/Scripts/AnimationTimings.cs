/// <summary>
/// Merkezi animasyon süreleri config dosyası
/// İnce ayar için bu değerleri değiştirebilirsin
/// </summary>
public static class AnimationTimings
{
    // ========== Animasyon Süreleri (saniye) ==========
    
    /// <summary>Fireball/Attack animasyonu toplam süresi</summary>
    public const float ATTACK_DURATION = 3.367f;
    
    /// <summary>Hit/Reaction animasyonu süresi</summary>
    public const float HIT_REACTION = 1.833f;
    
    /// <summary>Dizzy/Stun animasyonu süresi</summary>
    public const float STUN_DURATION = 4.267f;
    
    /// <summary>Defeat animasyonu süresi</summary>
    public const float DEFEAT_DURATION = 7.30f;
    
    /// <summary>Victory animasyonu süresi</summary>
    public const float VICTORY_DURATION = 4.50f;
    
    // ========== Zamanlama Ayarları ==========
    
    /// <summary>
    /// Attack animasyonu başladıktan kaç saniye sonra vuruş gerçekleşir
    /// Bu süre geçtikten sonra Hit animasyonu ve hasar uygulanır
    /// </summary>
    public const float ATTACK_HIT_DELAY = 1.5f;
    
    /// <summary>
    /// Yeni müşteri spawn olduktan sonra bekleme süresi
    /// Bu süre boyunca oyuncu durumu anlayabilir
    /// </summary>
    public const float SPAWN_DELAY = 2.0f;
    
    /// <summary>
    /// Tur geçişleri arasındaki kısa gecikme
    /// </summary>
    public const float TURN_TRANSITION_DELAY = 0.5f;
}
