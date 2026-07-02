# Vertigo Games — Technical Artist Demo
## Sarper Dündar

---

### Kurulum

- **Unity Versiyonu:** Unity 6 (6000.3.x)
- **Render Pipeline:** URP (Universal 3D Template)
- **Battle Pass Sahnesi:** `Assets/_Project/Scenes/UI_Demo.unity`
- **Weapon VFX Sahnesi:** `Assets/_Project/Scenes/VFX_Demo.unity`

Projeyi açtıktan sonra ilgili sahneyi yükleyin ve Play moduna geçin.

### Video Kayıtları

Tüm video kayıtları `Recordings/` klasöründedir (post-processing eklenmemiştir):

- `UI_VideoCapture.mp4` — Battle Pass akışının tamamı
- `VFX_VideoCapture_01/02/03.mp4` — Weapon VFX çekimleri

---

### Task 1 — Battle Pass Road

#### Test Edilecek Etkileşimler

1. **Yatay kaydırma:** Road, ScrollRect ile yatay kaydırılabilir. Ekran kenarlarındaki ok göstergeleri kaydırma yönünü belirtir. Mevcut seviye ekran dışına çıktığında sol kenarda sıradaki seviye göstergesi, sağ kenarda ise ultimate ödül önizlemesi görünür.
2. **Ücretsiz ödül alma:** Claimable (parlayan) veya unlocked durumdaki ücretsiz ödüllere tıklayın. Ödül popup'ı açılır; "Topla" butonuna basıldığında para birimi ikonları header'daki ilgili kutuya doğru uçar ve sayaç kademeli olarak artar.
3. **Seviye satın alma:** Progress bar'ın ucundaki elmas maliyetli butona tıklayın. Onay popup'ından sonra XP ikonları XP bar'ına uçar, bar dolar, seviye atlar ve progress bar yeni konumuna animasyonla ilerler. Maliyet her seviyede artar.
4. **Yetersiz elmas:** Elmas yetmediğinde satın alma popup'ı yerine mağaza popup'ı açılır. Üç farklı elmas paketi butonundan biri seçildiğinde elmaslar header'a uçarak eklenir.
5. **Ultimate ödül:** Road'un en sonunda, maksimum seviyeye ulaşılana kadar gri tonda bekleyen özel ödül bulunur. Maksimum seviyede renklenir.

#### Item Durumları (State FX)

| Durum | Görsel Karşılık |
|---|---|
| Locked | Koyu tint + kilit ikonu |
| Unlocked | Normal renk + bildirim rozeti (breathing animasyonu) |
| Claimable | Glow efekti + bildirim rozeti + parlama sweep'i |
| Claimed | Soluk tint + sağ alt köşede onay işareti |
| Premium | Her zaman kilitli görünüm, etkileşime kapalı (satın alma akışı kapsam dışı) |

Bildirim rozetleri sine tabanlı breathing scale animasyonu kullanır; her rozet rastgele faz ofsetiyle başlar, böylece senkron hareket etmez. Parlama sweep'leri de aynı şekilde rastgele zamanlamayla tekrarlar.

#### Mimari

- **Veri odaklı yapı:** `LevelData` / `RewardItemData` serileştirilebilir sınıfları Inspector'dan düzenlenir; kolonlar ve ödüller runtime'da prefab'lardan spawn edilir. Backend gerektirmez.
- **`BattlePassManager`** merkezi orkestratör: durum hesaplama, seviye atlama zinciri ve popup yönlendirmesi tek noktadan yönetilir.
- **Progress Road:** Segmentler seviye noktaları arasına runtime'da yerleştirilir; doluluk animasyonu coroutine + callback zinciriyle sıralanır (XP uçuşu → bar dolumu → segment dolumu → endpoint spawn).
- **Animasyon zincirleme:** Tüm ödül ve seviye akışları `Action onComplete` callback'leriyle bağlanır — animasyonlar üst üste binmez, para birimleri uçuş tamamlanmadan hesaba eklenmez.

#### UI Shader — Outline Glow (El Yazımı HLSL)

`UI_OutlineGlow.shader`, sprite'ın alpha kanalını 4 halka × 12 örnekle (48 tap) radyal olarak örnekleyerek ikon silüetinin dışına taşan emisyon efekti üretir. Unity UI stencil/mask sistemiyle tam uyumludur.

- **Sprite Padding** parametresi sprite'ı RectTransform içinde küçülterek glow'a alan açar (Full Rect mesh gerektirir)
- Halkalar mesafe ağırlıklı ve faz kaydırmalı — banding görünmez
- Premultiplied alpha blending ile additive görünüm
- Yalnızca seçili öğelerde kullanılır — tüm UI'a uygulanmaz, fragment maliyeti kontrollü tutulur

#### Performans Notları

| Konu | Yaklaşım |
|---|---|
| UI VFX | Particle System yerine UI Image + script animasyonları — Canvas ile sorting sorunu yok, overdraw öngörülebilir |
| Sprite'lar | Button ve frame'ler 9-slice (Sliced) — küçük dokularla her boyutta keskin kenar |
| Layout | Kolonlar tek seferde spawn edilir; scroll sırasında layout rebuild tetiklenmez |
| Uçuş efektleri | İkonlar havuz benzeri kısa ömürlü instantiate/destroy, aynı anda en fazla 8 ikon |
| Glow shader | 48 tap yalnızca glow'lu öğelerde; UI genelinde varsayılan UI/Default kullanılır |

---

### Task 2 — Weapon VFX

#### Sahne Açıklaması

Silah modeli sahnenin ortasında, koyu mavi radyal gradient arka plan önünde sergilenmektedir. HDRI studio cubemap ile silah yüzeyinde gerçekçi yansımalar sağlanmıştır. Ortam ışığı düşük tutularak emisyon ve additive efektlerin kontrastı korunmuştur.

#### Aura Efekti — Shader Graph (Çift Katmanlı UV Scroll)

Silahın etrafına sarılmış 7 adet şerit (strip) mesh, Blender'da bezier eğrilerinden modellenerek UV açılmıştır. U ekseni şerit uzunluğu boyunca, V ekseni şerit genişliği boyunca haritalanmıştır.

Tek bir Shader Graph üzerinde iki katmanlı UV scroll sistemi kurulmuştur:

- **Çizgi Katmanı (Line):** Keskin, dar, yüksek emisyonlu altın rengi doku. Hızlı scroll hızı. Enerji akışı hissi verir.
- **Aura Katmanı:** Yumuşak, geniş, düşük emisyonlu doku. Daha yavaş scroll hızı. Atmosferik derinlik sağlar.

Her iki katman aynı shader içinde bağımsız olarak scroll edilir ve sonuçlar toplanarak additive blending ile render edilir. Şerit uçlarında ve kenarlarında yumuşak geçiş (fade) uygulanmıştır:

- **Endpoint Fade:** Ham (scroll edilmemiş) UV.x kullanılarak şerit uçlarında sıfıra düşer. Doku bu sabit pencereden geçerek akar — uçlarda ani kesinti olmaz.
- **Width Fade:** UV.y kullanılarak şerit kenarları yumuşatılır.

Vertex displacement sine dalga ile Y ekseninde uygulanmıştır. Object Position node'u faz ofseti olarak kullanılarak her şerit farklı zamanlama ile hareket eder — aynı materyal üzerinde, ekstra maliyet olmadan.

Üç farklı materyal kombinasyonu kullanılmıştır:

| Materyal | Açıklama | Kullanım |
|---|---|---|
| MAT_Aura_Only | Sadece aura katmanı aktif (_LineAlpha = 0) | 2 şerit |
| MAT_Line_Only | Sadece çizgi katmanı aktif (_AuraAlpha = 0) | 2 şerit |
| MAT_Aura_Line | Her iki katman aktif | 3 şerit |

**Shader Ayarları:**
- Surface Type: Transparent
- Blending: Additive
- Render Face: Both (Cull Off)
- ZWrite: Off
- Unlit — emisyon, HDR renk değerleri ile BaseColor üzerinden sağlanır

#### Parçacık Efektleri (Particle Systems)

Üç adet parçacık sistemi silahın farklı noktalarına yerleştirilmiştir. Hepsi silah kök objesinin child'ıdır — silah hareket ettiğinde birlikte hareket ederler. Tüm sistemlerde Size over Lifetime aktiftir — parçacıklar ömürleri boyunca büyür.

- **Taç Parçacıkları (Crown):** Silahın üst taç dekorasyonundan yıldız şeklinde parçacıklar yayar. Cone Shape kullanır. Emission rate: 20, duration: 0.5s, Start Speed: 0.25–0.3. Angular Velocity her eksende rastgele 360°–540° arası dönüş — parçacıklar havada dönerek dağılır.
- **Namlu Parçacıkları (Gun Point):** Silahın namlu kısmından yıldız şeklinde parçacıklar yayar. Blender'da orijinal silah modelinden türetilmiş görünmez bir emisyon mesh'i kullanır — Mesh Renderer devre dışı bırakılarak mesh görünmez yapılmıştır, Shape modülü Mesh → Surface modunda bu mesh'ten emit eder. Emission rate: 10, duration: 1s, Start Speed: 0. Angular Velocity: ~10° ömür boyunca. Taç parçacıklarıyla aynı materyali paylaşır.
- **Alt Parçacıklar (Below):** Silahın altından Unity varsayılan parçacık formuyla yayılır. Start Speed: 0.5–1. Ayrı bir materyal kullanır.

İki ayrı additive materyal kullanılmıştır (URP → Particles → Unlit, Additive blend). Taç ve namlu sistemleri aynı yıldız dokulu materyali, alt sistem farklı bir materyali kullanır. Parçacık rengi ve HDR emisyon yoğunluğu her sistemin Start Color ayarından kontrol edilir.

**Ortak Ayarlar:**
- Min Particle Size: 0 (uzaklaşırken büyüme sorunu önlenir)
- Size over Lifetime: aktif (parçacıklar büyür)
- Billboard render, additive blend

#### Performans Notları

| Metrik | Değer |
|---|---|
| Aura shader | Fragment başına 2 doku örneklemi, vertex displacement basit sine hesaplaması |
| Şerit mesh'leri | Toplam ~800 vertex altında, 7 şerit |
| Materyal sayısı | 3 aura materyali + 2 parçacık materyali = 5 toplam |
| Parçacık bütçesi | Taç: rate 20 / 0.5s duration, Namlu: rate 10 / 1s duration, Alt: düşük rate — toplam aktif parçacık sayısı düşük |
| Doku boyutları | Aura dokuları 128×128, parçacık dokuları 32×32, arka plan 512×512. Tüm doku max size'ları ihtiyaca göre ayarlanmıştır |
| Doku sıkıştırma | Platform varsayılan format (Automatic) |
| Vertex displacement | Fragment manipülasyonu yok, sadece vertex — GPU maliyeti minimum |
| Overdraw | Şeritler ince, parçacıklar küçük — additive overdraw minimal |

---

### Proje Yapısı

```
Assets/
  _Project/
    VertigoAssets/         ← Vertigo tarafından sağlanan dosyalar (değiştirilmemiş)
      UI/                    Sprite'lar ve referanslar
      VFX/                   Silah modeli, dokular ve referanslar
    Materials/
      UI/                    Outline glow materyali
      VFX/                   Aura ve parçacık materyalleri
    Models/
      VFX/                   Blender'da modellenen şerit mesh'leri
    Prefabs/
      UI/                    Battle Pass prefab'ları (kolon, ödül, endpoint...)
      VFX/                   Parçacık sistem prefab'ları
    Scripts/
      BattlePass/            Battle Pass sistem scriptleri
      Utils/                 Yeniden kullanılabilir animasyon scriptleri
    Shaders/
      UI/                    UI_OutlineGlow (el yazımı HLSL)
      VFX/                   Aura_DualScroll (Shader Graph)
    Textures/
      VFX/                   Scroll, parçacık ve arka plan dokuları
    Scenes/
      UI_Demo.unity
      VFX_Demo.unity
```

VertigoAssets klasörü Vertigo'nun sağladığı orijinal dosyaları içerir, değiştirilmemiştir. Geri kalan tüm klasörler benim implementasyonumdur.

---

### Kullanılan Araçlar

- Unity 6 (URP)
- Blender (şerit mesh modelleme ve UV açma)
- Substance Designer (scroll ve parçacık dokuları)

---

*Sarper Dündar — Temmuz 2026*
