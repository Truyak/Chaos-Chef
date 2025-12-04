# 🤵 Chaos Chef

Bu projeyi tarayıcı üzerinden hemen oyna: 🔗 [OYUNU OYNA](https://truyak.itch.io/chaos-chef)

---

## 🎮 Proje Hakkında
**Chaos Chef**, Unity oyun motoru kullanılarak geliştirilmiş, stratejik öğeler barındıran sıra tabanlı bir kart oyunudur. Oyunda klasik savaşçı arketipleri yerine, zorlu bir müşteriyle başa çıkmaya çalışan bir **Garson'u** yönetiyorsunuz.

**Amaç:** "Müşteri her zaman haklıdır" kuralını yıkmak! Müşterinin sabrını (canını) tüketerek masadan mutlu (veya pes etmiş) kalkmasını sağlamak ve kalan sabrımız kadar **Dolar ($)** cinsinden bahşiş toplamaktır.

---

## 🛠️ Oynanış ve Mekanikler
Oyun, karşılıklı hamle sırasına (**Turn-Based**) dayanır. Kod altyapısında **Poison** (Zehir), **Stun** (Sersemletme), **Debuff** (Zayıflatma) ve **Damage** (Hasar) olmak üzere 4 temel etki tipi bulunur.

### 🃏 Oyuncu (Garson) Yetenekleri:
Oyuncu destesi, rastgele çekilen kartlardan oluşur ve şu etkileri içerir:

* **Sıcak Servis (Damage):** Müşterinin sabrını doğrudan azaltan ana hasar kaynağı.
* **Midesini Bozma (Poison):** Yüksek hasar potansiyeli taşıyan ancak stratejik kullanım gerektiren kartlar.
* **İkram (Heal/Buff):** Garsonun stres seviyesini düşürür (Can yeniler).
* **Oyalama Taktikleri (Stun/Debuff):** Müşteriyi bekletir veya sonraki hamlesinin etkisini azaltır.

### 😡 Rakip (Müşteri) Davranışları (AI):
Rakip, `GameManager` içinde tanımlanmış özel bir karar mekanizmasıyla rastgele şu aksiyonlardan birini seçer:

* **Şikayet Fırtınası:** Garsona doğrudan yüksek stres (hasar) yükler.
* **Alerji Tuzağı (Stun + Hasar):** Garsonu paniğe sürükler, hem hasar verir hem de bir tur kilitleyerek (Stun) hamle yapmasını engeller.
* **Kötü Yorum Tehdidi (Poison):** Zamanla hasar veren (DoT) bir etki bırakır. Her tur başında garsonun canı azalır.
* **Hesap Şoku (Debuff):** Hesabı incelemeye başlar. Garsonun bir sonraki saldırısının etkisini **%40** oranında düşürür (`customerDebuffMultiplier`).

> **Not:** Rakip karakter şu aşamada manuel çalışan bir yapıdadır. İlerleyen aşamalarda **ML-Agents** entegrasyonu için gerekli altyapı (State machine) hazırlanmıştır.

---

## ⚙️ Teknik Özellikler
Proje, "Clean Code" prensiplerine uygun olarak modüler bir yapıda tasarlanmıştır:

* **GameManager:** Oyun döngüsünü (Turn System), can değerlerini ve kazanma/kaybetme durumlarını yönetir. Singleton tasarım deseni kullanılmıştır.
* **Card System:** `ScriptableObject` kullanılarak kart verileri (`CardData`) modüler hale getirilmiş, yeni kart eklemek kod yazmadan mümkün kılınmıştır.
* **Audio Manager:** `AudioMixer` entegrasyonu ile Müzik ve SFX kanalları ayrı ayrı kontrol edilebilir.
* **UI Management:** Dinamik HP barları, durum ikonları (Zehir, Stun vb. görselleri) ve menü geçişleri `UIManager` tarafından kontrol edilir.

---

## 🎛️ Menü ve Ayarlar
* **Ana Menü:** Oyuna giriş ve çıkış işlemleri.
* **Ayarlar (Options):** Müzik ve Efekt sesleri sliderlar aracılığıyla gerçek zamanlı (Logaritmik dB dönüşümü ile) ayarlanabilir.

---

## 📦 Kurulum ve Dosya Yapısı
Bu repo, projenin kaynak kodlarını içerir.

* **Unity Versiyonu:** 2021.3.x (LTS)

**Projeyi kendi bilgisayarınızda çalıştırmak için:**
1.  Repoyu klonlayın.
2.  Unity Hub üzerinden projeyi "Add" diyerek ekleyin.
3.  Unity, gerekli kütüphaneleri otomatik olarak oluşturacaktır.
