# Chaos Chef 🍳

![Chaos Chef Logo](chaos_chef_logo.png)

🎮 **[Oyunu Oyna - itch.io](https://truyak.itch.io/chaos-chef)**

Restoran temalı bir kart savaş oyunu! Garson olarak sinirli müşterilere karşı savunma yapın!

## 🎮 Oyun Hakkında

**Chaos Chef**, 2D kart tabanlı bir sıra tabanlı strateji oyunudur. Oyuncu bir garson rolünde, restorana gelen zorlu müşterilere karşı yemek temalı kartlarla savaşır.

### Oynanış
- **Kart Oynama**: Her turda stamina harcayarak kart oynayın
- **Stamina Yönetimi**: Her kart farklı stamina maliyetine sahip
- **Özel Efektler**: Stun, Poison, Debuff gibi efektlerle avantaj kazanın
- **Level Sistemi**: Müşterileri yenerek yeni seviyelere geçin
- **Kart Koleksiyonu**: Coin kazanarak yeni kartlar açın ve destenizi oluşturun

---

## 🤖 Yapay Zeka Sistemi (Detaylı)

Bu oyun **Q-Learning** tabanlı bir yapay zeka içerir. Müşteri (düşman) karakteri, takviyeli öğrenme (Reinforcement Learning) algoritmasıyla eğitilmiş bir ajan tarafından kontrol edilir.

### Q-Learning Algoritması

Q-Learning, bir ajanın deneme-yanılma yoluyla optimal davranışları öğrendiği bir takviyeli öğrenme yöntemidir.

| Parametre | Değer | Açıklama |
|-----------|-------|----------|
| Learning Rate (α) | 0.1 | Yeni bilginin ne kadar hızlı öğrenileceği |
| Discount Factor (γ) | 0.95 | Gelecekteki ödüllerin önemi |
| Exploration Rate (ε) | 0.3 → 0.05 | Rastgele keşif oranı (zamanla azalır) |
| Exploration Decay | 0.995 | Her episode'da ε'un azalma oranı |

**Q-Table Güncelleme Formülü:**
```
Q(s,a) = Q(s,a) + α × [R + γ × max(Q(s',a')) - Q(s,a)]
```

---

### 🎯 Durum (State) Kodlaması

AI, oyun durumunu 7-bit bir integer olarak kodlar (0-127 arası değer):

| Bit Pozisyonu | Bilgi | Kodlama |
|---------------|-------|---------|
| Bit 0-1 | Oyuncu HP | 4 seviye (0-25%, 25-50%, 50-75%, 75-100%) |
| Bit 2-3 | Müşteri (AI) HP | 4 seviye |
| Bit 4 | Oyuncu Stun | 0 = Yok, 1 = Var |
| Bit 5 | Oyuncu Poison | 0 = Yok, 1 = Var |
| Bit 6 | Oyuncu Debuff | 0 = Yok, 1 = Var |

Bu kompakt kodlama sayesinde AI sadece 128 farklı durumu öğrenmesi gerekir.

---

### ⚔️ AI Aksiyonları (8 Adet)

Müşteri AI'ı 8 farklı restoran temalı saldırı kullanabilir:

| # | Aksiyon Adı | Hasar | Efekt | Süre | Cooldown |
|---|-------------|-------|-------|------|----------|
| 0 | Şikayet Fırtınası | 25 | Damage | - | 1 tur |
| 1 | Küçük Şikayet | 12 | Damage | - | 1 tur |
| 2 | Alerji Tuzağı | 15 | **Stun** | 1 tur | 3 tur |
| 3 | Kötü Yorum Tehdidi | 5 | **Poison** | 2 stack | 1 tur |
| 4 | Hesap Şoku | 0 | **Debuff** | 2 tur | 2 tur |
| 5 | Sosyal Medya Tehdidi | 20 | **Poison** | 1 stack | 1 tur |
| 6 | Yöneticiyi Çağırma | 30 | Damage | - | 2 tur |
| 7 | Pasif Agresif Bakış | 8 | **Debuff** | 1 tur | 1 tur |

**Efekt Açıklamaları:**
- **Stun**: Oyuncu bir tur boyunca hareket edemez
- **Poison**: Her tur başında 10 hasar verir
- **Debuff**: Oyuncunun verdiği hasar %40 azalır

---

### 🎁 Ödül (Reward) Sistemi

AI, her aksiyonun ardından aşağıdaki ödülleri alır:

| Durum | Ödül |
|-------|------|
| Hasar verme | `hasar / 10` puan |
| Stun uygulama | +25 puan |
| Poison uygulama | +15 puan |
| Debuff uygulama | +12 puan |
| Aynı aksiyonu üst üste yapma | **-15 puan (ceza)** |
| Oyunu kazanma | +100 puan |
| Oyunu kaybetme | -100 puan |

**Tekrar Cezası**: AI aynı aksiyonu üst üste yaparsa -15 puan alır. Bu, AI'ın spam yapmasını engeller ve çeşitli stratejiler geliştirmesini teşvik eder.

---

### 🔄 Cooldown Mekanizması

Her aksiyon kullanıldıktan sonra belirli bir süre tekrar kullanılamaz:

- Aksiyon kullanıldığında `currentCooldown = cooldownTurns` olarak ayarlanır
- Her tur sonunda tüm aksiyonların cooldown'u 1 azaltılır
- `currentCooldown <= 0` olduğunda aksiyon tekrar kullanılabilir
- Hiç kullanılabilir aksiyon kalmazsa tüm cooldown'lar sıfırlanır (edge case)

---

### 🎲 Aksiyon Seçimi (ε-Greedy Policy)

AI aksiyonu şu şekilde seçer:

```
if (training mode && random() < explorationRate):
    → Rastgele kullanılabilir aksiyon seç (keşif)
else:
    → En yüksek Q-değerine sahip kullanılabilir aksiyonu seç (sömürü)
```

- **Eğitim modunda**: Yüksek exploration rate ile farklı stratejiler dener
- **Oyun modunda**: Düşük exploration rate ile öğrendiği en iyi stratejiyi uygular

---

### 🏋️ Eğitim Süreci (AITrainer)

AI, `AITrainer.cs` ile şu şekilde eğitilir:

1. **Simülasyon**: Sanal oyuncu (`TrainingPlayer`) ile binlerce tur oynanır
2. **Oyuncu Stratejisi**: 
   - %50 rastgele kart seçimi (AI'a öğrenme şansı tanır)
   - %30 stun kartı önceliği
   - Düşük HP'de yüksek hasarlı kart
3. **Episode**: Her oyun bir episode olarak sayılır
4. **Kayıt**: Her 100 episode'da Q-Table kaydedilir
5. **Hedef**: 1000+ episode ile doygunluğa ulaşılır

**Eğitim İstatistikleri:**
- Toplam oyun sayısı
- AI kazanma oranı
- Q-Table state sayısı
- Stun/Poison kullanım oranları

---

### 💾 Model Kayıt/Yükleme

| Yöntem | Açıklama |
|--------|----------|
| PlayerPrefs | Devam eden eğitim/oyun için |
| JSON Export | `CustomerAI_Model.json` dosyasına pretrained model |
| Resources | Build içinde gömülü model |

**Yükleme Önceliği:**
1. PlayerPrefs (varsa)
2. Pretrained JSON model
3. Boş Q-Table (yeni başlangıç)

---

### 🎮 Oyun İçi Entegrasyon

`GameManager.cs` AI'ı şu şekilde kullanır:

```csharp
// 1. Mevcut durumu hesapla
int state = customerAI.GetStateHash(playerHP, customerHP, maxHP, 
                                     stunTurns, poisonStacks, debuffTurns);

// 2. En iyi aksiyonu seç
int actionIndex = customerAI.SelectAction(state);

// 3. Aksiyonu uygula
AIAction action = customerAI.ExecuteAction(actionIndex);

// 4. Hasar ve efektleri uygula
playerHP -= action.damage * customerDebuffMultiplier;

// 5. Q-Table'ı güncelle
float reward = customerAI.CalculateReward(damage, action.effectType);
customerAI.UpdateQTable(newState, reward, isTerminal);
```

---

## 📁 Proje Yapısı

```
Chaos-Chef/
├── CustomerAI_Model.json     # Eğitilmiş AI ağırlıkları (Q-Table)
├── chaos_chef_logo.png       # Oyun logosu
├── Assets/_Game/
│   ├── Scripts/
│   │   ├── AI/               # AI implementasyonu
│   │   │   ├── CustomerAI.cs     # Q-Learning algoritması
│   │   │   ├── AIAction.cs       # Aksiyon tanımları
│   │   │   ├── AITrainer.cs      # Eğitim sistemi
│   │   │   └── TrainingPlayer.cs # Simülasyon oyuncusu
│   │   ├── GameManager.cs    # Ana oyun döngüsü
│   │   └── ActionProjectile.cs   # Saldırı görselleri
│   ├── Resources/AI/         # Build için gömülü model
│   └── Sprites/Projectiles/  # Saldırı ikonları
└── Builds/                   # Oyun build'leri
```

---

## 🔧 Kontroller

- **Kartlara tıklayın** - Kart oynamak için
- **Turu Bitir** - Sıranızı bitirmek için
- **AI Toggle** - Ana menüde AI modunu değiştirmek için
