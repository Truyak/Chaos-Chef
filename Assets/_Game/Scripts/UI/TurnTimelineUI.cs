using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TurnTimelineUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform container; // Horizontal Layout Group olan panel
    public GameObject slotPrefab; // İsteğe bağlı, eğer dinamik yaratılacaksa
    public Image[] slotImages; // Editörden sürükle-bırak yapılacak slotların Image'leri (Icon image'leri)

    [Header("Icons")]
    public Sprite playerIcon;
    public Sprite customerIcon; // Varsayılan ikon, SetCustomerIcon ile değişebilir

    public void SetCustomerIcon(Sprite icon)
    {
        if (icon != null)
        {
            customerIcon = icon;
            // Mevcut durumu güncellemek istersen:
            // UpdateTimeline(...) çağrılabilir ama parametreleri bilmiyoruz.
            // O yüzden bir sonraki güncellemede otomatik değişecek.
        }
    }

    private void Start()
    {
        // Başlangıçta slotları bulmaya çalış (eğer atanmadıysa)
        if (slotImages == null || slotImages.Length == 0)
        {
            // Container altındaki tüm Image'leri bul (ama sadece ikon olanları)
            // Bu kısım kullanıcının hiyerarşisine göre değişebilir, o yüzden 
            // Inspector'dan atamak en garantisi.
        }
    }

    /// <summary>
    /// Timeline'ı günceller.
    /// </summary>
    /// <param name="isPlayerTurn">Şu an kimin sırası?</param>
    /// <param name="playerStunTurns">Oyuncunun kalan stun süresi</param>
    /// <param name="customerStunTurns">Müşterinin kalan stun süresi</param>
    public void UpdateTimeline(bool isPlayerTurn, int playerStunTurns, int customerStunTurns)
    {
        if (slotImages == null || slotImages.Length == 0) return;

        List<bool> turnOrder = CalculateNextTurns(isPlayerTurn, playerStunTurns, customerStunTurns, slotImages.Length);

        for (int i = 0; i < slotImages.Length; i++)
        {
            if (i < turnOrder.Count)
            {
                slotImages[i].sprite = turnOrder[i] ? playerIcon : customerIcon;
                slotImages[i].gameObject.SetActive(true);
            }
            else
            {
                slotImages[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Gelecek turların kime ait olduğunu hesaplar.
    /// True = Player, False = Customer
    /// </summary>
    private List<bool> CalculateNextTurns(bool currentIsPlayer, int pStun, int cStun, int count)
    {
        List<bool> turns = new List<bool>();
        
        // Simülasyon değişkenleri
        bool simIsPlayerTurn = currentIsPlayer;
        int simPStun = pStun;
        int simCStun = cStun;

        // "Şu anki" tur zaten oynanıyor, o yüzden timeline GELECEK turları göstermeli.
        // O yüzden simülasyona bir sonraki turdan başlamalıyız.
        // ANCAK: Kullanıcı "Sıraları gösteriyor" dedi, yani şu anki tur da dahil olabilir.
        // Genelde Timeline en solda "Şu anki" turu gösterir. Biz de öyle yapalım.

        for (int i = 0; i < count; i++)
        {
            // Stun kontrolü - Eğer sırası gelen kişi stunlıysa, turu atlar
            bool turnOwner = simIsPlayerTurn;
            bool isSkipped = false;

            if (simIsPlayerTurn)
            {
                if (simPStun > 0)
                {
                    simPStun--;
                    isSkipped = true;
                }
            }
            else
            {
                if (simCStun > 0)
                {
                    simCStun--;
                    isSkipped = true;
                }
            }

            if (isSkipped)
            {
                // Tur atlandı, sıra diğerine geçti
                // Ama bu "atlanan" tur timeline'da görünmeli mi?
                // Genelde görünmez, direkt bir sonraki oynayan görünür.
                // Stun yiyen kişinin sırası kayar.
                
                // Tekrar döngü başı yapmadan turu değiştirip devam etmeliyiz
                simIsPlayerTurn = !simIsPlayerTurn;
                
                // Bu indeksi tekrar hesaplamak için i'yi azaltıyoruz
                i--; 
                continue; 
                // DİKKAT: Sonsuz döngü riski (ikisi de sonsuza kadar stunlıysa).
                // Ama stun süreleri azalıyor, o yüzden risk yok.
            }

            // Tur oynanıyor
            turns.Add(turnOwner);
            
            // Sıra değişir
            simIsPlayerTurn = !simIsPlayerTurn;
        }

        return turns;
    }
}
