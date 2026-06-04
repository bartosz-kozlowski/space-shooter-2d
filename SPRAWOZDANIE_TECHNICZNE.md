# Sprawozdanie Techniczne

## Autorzy: Bartosz Kozłowski, Paweł Kolec

## Space Shooter 2D

## 1. Opis Projektu

**2D kosmiczna strzelanka** z perspektywą pionową (top-down shooter). Gracz wybiera jeden z dwóch statków kosmicznych i odpiera kolejne fale przeciwników, zdobywając punkty za każdego zniszczonego wroga. W trakcie rozgrywki pojawiają się power-upy oraz dostępne jest menu ulepszeń, które pozwala trwale wzmacniać statek kosztem zgromadzonych punktów. Gra kończy się gdy HP gracza spadnie do zera — wyświetlany jest wówczas ekran końcowy z wynikiem i opcją restartu.

---

## 2. Pełny Game Loop

### 2.1 Przepływ Scen

```
┌─────────────┐     START GAME     ┌──────────────────┐
│  MainMenu   │ ────────────────►  │  CharSelectScene  │
└─────────────┘                    └────────┬─────────┘
       ▲                                    │ wybór statku
       │  RESTART / MAIN MENU               ▼
       │                           ┌──────────────────┐
       │                           │    GameScene      │
       │                           │  (właściwa gra)   │
       │                           └────────┬─────────┘
       │                                    │ HP = 0
       │        ┌──────────────────┐        │ (2s opóźnienie)
       └──────── │    GameOver      │ ◄──────┘
                 └──────────────────┘
```

Przejścia między scenami obsługuje `LevelManager.cs` za pomocą `SceneManager.LoadScene()`. Przejście do sceny `GameOver` jest celowo opóźnione o 2 sekundy koroutyną `WaitAndLoad()` — daje to czas na odtworzenie animacji eksplozji statku gracza zanim nastąpi zmiana sceny.

### 2.2 Odpowiedzialności Scen

| Scena | Zawartość |
|---|---|
| `MainMenu` | Przyciski: Graj, Wyjdź |
| `CharSelectScene` | Wybór statku: Blue Ship / Orange Ship |
| `GameScene` | Rozgrywka — HUD, fale wrogów, power-upy, menu ulepszeń |
| `GameOver` | Wyświetlenie finalnego wyniku, przyciski Restart i Menu |

---

## 3. Dwie Grywalne Postacie

Gra oferuje dwa statki o odmiennych właściwościach i wyglądzie UI.

### 3.1 Blue Ship

- **Czas ładowania Power Shota:** 6 sekund
- **Kolor HUD:** gradient cyan–niebieski (`#49FFF3` → `#6BABFF`)
- **Pasek ładowania:** niebieskie klatki animacji (`blueFrames`)
- **Pasek zdrowia:** sprite dedykowany dla niebieskiego statku

### 3.2 Orange Ship

- **Czas ładowania Power Shota:** 3.5 sekundy (szybsze ładowanie)
- **Kolor HUD:** pomarańczowy (`#FF8000`)
- **Pasek ładowania:** pomarańczowe klatki animacji (`orangeFrames`)
- **Pasek zdrowia:** sprite dedykowany dla pomarańczowego statku

### 3.3 Mechanizm Wyboru i Persistencji

Wybór statku dokonywany jest w `CharSelectScene` przez przyciski UI podłączone do skryptu `CharSelect.cs`. Wybrany indeks statku jest przechowywany przez `CharSelectManager` (Singleton z `DontDestroyOnLoad`), dzięki czemu informacja o wybranym statku jest dostępna w `GameScene` bez użycia `PlayerPrefs`. Na podstawie indeksu:

- `PlayerInstantiate.cs` — tworzy właściwy prefab statku (wzorzec Factory)
- `ChargeBarManager.cs` — wybiera odpowiedni zestaw klatek i czas ładowania
- `HPBarManager.cs` — ustawia właściwy sprite paska zdrowia
- `UIUpdater.cs` — aplikuje schemat kolorystyczny tekstu wyniku

---

## 4. Architektura Kodu — Wzorce Projektowe

### 4.1 Singleton

**Zastosowanie:** `AudioManager`, `ScoreKeeper`, `CharSelectManager`

Wzorzec gwarantuje dokładnie jedną instancję menedżera dostępną globalnie. Każda klasa implementuje sprawdzenie w `Awake()` i wywołuje `DontDestroyOnLoad()`, dzięki czemu obiekt przeżywa zmianę sceny.

```csharp
// AudioManager.cs
public static AudioManager Instance { get; private set; }

void Awake()
{
    if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
    else Destroy(gameObject);
}
```

| Singleton | Odpowiedzialność | Persystuje między scenami |
|---|---|---|
| `AudioManager` | Odtwarzanie SFX, komunikacja z AudioMixerem | Tak |
| `ScoreKeeper` | Przechowywanie i modyfikacja wyniku | Tak |
| `CharSelectManager` | Przechowywanie indeksu wybranego statku | Tak |

**Uzasadnienie:** Te trzy systemy muszą istnieć przez cały czas życia gry i być dostępne z dowolnego skryptu bez przekazywania referencji przez Inspector.

---

### 4.2 Observer (Zdarzenia C#)

**Zastosowanie:** `ChargeBarManager` → `PlayerController` oraz `UpgradeManager` → `UpgradeMenuController`

Obiekty komunikują się przez zdarzenia (`event Action`), nie przez bezpośrednie referencje. Producent zdarzenia nie wie nic o swoich konsumentach.

```csharp
// ChargeBarManager.cs — producent
public event System.Action OnChargeFull;
OnChargeFull?.Invoke();   // odpala się gdy pasek osiągnie 100%

// PlayerController.cs — konsument
chargeBarManager.OnChargeFull += OnChargeBarFull;
```

```csharp
// UpgradeManager.cs — producent
public event Action OnUpgradeChanged;
OnUpgradeChanged?.Invoke();   // po każdym zakupie ulepszenia

// UpgradeMenuController.cs — konsument
upgrades.OnUpgradeChanged += RefreshLabels;
```

**Uzasadnienie:** `ChargeBarManager` nie powinien znać `PlayerController` — zdarzenie odwraca zależność. `UpgradeManager` nie powinien wiedzieć o istnieniu UI — emituje event i to UI decyduje jak zareagować.

---

### 4.3 State Pattern (Maszyna Stanów)

**Zastosowanie:** `PlayerController.cs` + `Shooter.cs`

Strzelanie gracza jest kontrolowane przez trzy wzajemnie wykluczające się stany. Każda klatka metoda `FireShooter()` sprawdza aktywny stan i wykonuje odpowiednie akcje.

```
[przycisk wciśnięty]          [pasek pełny]           [przycisk puszczony]
      │                             │                         │
      ▼                             ▼                         ▼
isPendingHold = true   ──►   isCharging = true   ──►   chargeFiring = true
(blokuje normalny ogień)   (animacja ładowania)     (oddaj Power Shot)
```

| Stan | Zmienna | Zachowanie |
|---|---|---|
| Normalny ogień | `isFiring` | Ciągłe strzały podczas przytrzymania przycisku |
| Oczekiwanie | `isPendingHold` | Przycisk wciśnięty, pasek się ładuje, brak strzałów |
| Naładowany | `isCharging` | Pasek pełny, statek drży, czeka na zwolnienie przycisku |

**Uzasadnienie:** Bez maszyny stanów warunki strzelania tworzyłyby nieczytelną sieć zagnieżdżonych `if-else`. Wyraźne stany pozwalają dodawać nowe zachowania bez modyfikacji istniejących.

---

### 4.4 Factory

**Zastosowanie:** `PlayerInstantiate.cs`, `PowerUpSpawner.cs`

Fabryka enkapsuluje logikę tworzenia obiektów — kod wywołujący nie zna prefabów ani warunków wyboru.

```csharp
// PlayerInstantiate.cs — fabryka statków
public GameObject CreateShip(int shipIndex)
{
    return Instantiate(shipPrefabs[shipIndex]);
}
```

`PlayerInstantiate` w `Awake()` pyta `CharSelectManager` o indeks statku i tworzy odpowiedni prefab. Klient (`CharSelectManager`) nie wie nic o tablicy prefabów.

`PowerUpSpawner.CreatePowerUpDrop()` działa analogicznie — losuje power-up z listy i tworzy instancję, enkapsulując cały system probabilistyczny przed resztą kodu.

**Uzasadnienie:** Dodanie trzeciego statku wymaga tylko dorzucenia prefabu do tablicy w Inspektorze — żaden inny skrypt nie wymaga zmiany.

---

### 4.5 Strategy (przez dziedziczenie ScriptableObject)

**Zastosowanie:** `PowerUpSO` → `HealthPowerUp`, `ShieldPowerUp`, `MultiShotPowerUp`

Abstrakcyjna klasa bazowa definiuje interfejs (`Apply()`). Konkretne strategie implementują własne zachowanie.

```csharp
// PowerUpSO.cs — interfejs strategii
public abstract class PowerUpSO : ScriptableObject
{
    public abstract void Apply(GameObject player);
}

// HealthPowerUp.cs — konkretna strategia
public override void Apply(GameObject player)
{
    player.GetComponent<Health>().Heal(healAmount);
}
```

`PowerUpManager` wywołuje `powerUp.Apply(player)` nie wiedząc z którym typem power-upa ma do czynienia — polimorfizm wybiera właściwą implementację.

**Uzasadnienie:** Dodanie nowego power-upa wymaga wyłącznie nowej klasy dziedziczącej po `PowerUpSO` — kod `PowerUpManager` pozostaje bez zmian.

---

## 5. Scriptable Objects

Dane konfiguracyjne są przechowywane jako zasoby `.asset` edytowalne w Inspektorze Unity, bez konieczności ingerencji w kod C#.

| Klasa | Atrybut `CreateAssetMenu` | Przechowywane dane |
|---|---|---|
| `WaveConfigSO` | `"New WaveConfig"` | Prefaby wrogów, waypoints trasy, prędkość, czasy spawnu |
| `HealthPowerUp` | `"HealthPowerUp"` | Ilość leczenia (`healAmount`) |
| `ShieldPowerUp` | `"ShieldPowerUp"` | Logika aktywacji tarczy |
| `MultiShotPowerUp` | `"MultiShotPowerUp"` | Liczba dodatkowych pocisków, czas trwania efektu |

**Uzasadnienie:** Projektant gry może modyfikować parametry fal wrogów, siłę power-upów i timing spawnu bezpośrednio w edytorze Unity — bez rebuildu projektu i bez ryzyka błędu w kodzie.

---

## 6. Fizyka i Interakcje

Projekt korzysta wyłącznie z komponentów 2D silnika fizycznego Unity.

### 6.1 Rigidbody2D

Prędkość pocisków i power-upów jest ustawiana przez `linearVelocity` na komponencie `Rigidbody2D`:

```csharp
// Shooter.cs — nadanie prędkości pociskowi
Rigidbody2D projectileRB = projectile.GetComponent<Rigidbody2D>();
projectileRB.linearVelocity = upVector * speed;
```

### 6.2 OnTriggerEnter2D

Wszystkie kolizje w grze są realizowane przez triggery (obiekty przenikają się fizycznie, trigger tylko sygnalizuje zdarzenie):

**Health.cs** — trafienie pocisku w statek:
```csharp
void OnTriggerEnter2D(Collider2D collision)
{
    DamageDealer damageDealer = collision.GetComponent<DamageDealer>();
    if (damageDealer != null)
    {
        TakeDamage(damageDealer.GetDamage());
        PlayHitParticles();
        damageDealer.Hit();           // niszczy pocisk
        audioManager.PlayTakeDamageSFX();
        cameraShake.Play();
    }
}
```

**PowerUpManager.cs** — zebranie power-upa przez gracza:
```csharp
void OnTriggerEnter2D(Collider2D collision)
{
    if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
    {
        powerUp.Apply(collision.gameObject);   // polimorficzny efekt
        Destroy(gameObject);
    }
}
```

### 6.3 Tarcza (Shield)

Tarcza absorbuje jedno trafienie i dezaktywuje się. Jest to realizowane przez flagę `isShieldActive` w `Health.cs` — gdy tarcza jest aktywna, obrażenia są zerowane i natychmiast wywoływane jest `DeactivateShield()`. Collider tarczy (`CircleCollider2D`) jest włączany i wyłączany przez `ShieldAnimationManager`.

---

## 7. Instancjonowanie Obiektów

Dynamiczne tworzenie obiektów w trakcie gry:

| Skrypt | Tworzony obiekt | Metoda |
|---|---|---|
| `Shooter.cs` | Pociski zwykłe i naładowane | `CreateProjectiles()` w koroutynie i jednorazowo |
| `EnemySpawner.cs` | Wrogowie | `Instantiate()` w koroutynie `SpawnEnemies()` |
| `PowerUpSpawner.cs` | Power-upy | `CreatePowerUpDrop()` po śmierci wroga |
| `Health.cs` | Efekty cząsteczkowe (hit particles) | `Instantiate()` przy każdym trafieniu |
| `PlayerInstantiate.cs` | Statek gracza | `CreateShip()` w `Awake()` |

Każdy tworzony obiekt ma zdefiniowany czas życia przez `Destroy(obj, seconds)`, co zapobiega akumulacji obiektów w pamięci.

---

## 8. System Walki

### 8.1 Normalny Ogień

Przytrzymanie przycisku Fire uruchamia koroutynę `FireContinously()` w `Shooter.cs`, która w pętli tworzy pociski z odstępem określonym przez `baseFireRate ± fireRateVariance`. Czas między strzałami jest skalowany mnożnikiem z ulepszenia RELOAD.

### 8.2 Power Shot (Naładowany Strzał)

Mechanika ładowania strzału łączy kilka systemów:

```
Wciśnięcie Fire
      │
      ├─ [0.2s opóźnienie]
      │
      ▼
ChargeBarManager.PlayChargeBarAnimation()
      │  animacja paska klatka-po-klatce
      │  efektywny czas = maxChargeTime × mnożnik RELOAD
      │
      ▼ (100% paska)
OnChargeFull event
      │
      ▼
PlayerController.OnChargeBarFull()
      │  isCharging = true
      │  ChargingAnimationManager.PlayChargingAnimation()
      │  AudioManager.PlayChargeUpSFX()
      │
Zwolnienie Fire
      │
      ▼
Shooter.ChargeFire()
      │  osobny prefab (chargeProjectilePrefab)
      │  wyższa prędkość (chargedProjectileSpeed)
      │  AudioManager.PlayChargingShotSFX()
      ▼
StopChargingAnimation()
```

Opóźnienie 0.2s przed uruchomieniem paska zapobiega przypadkowemu inicjowaniu ładowania przy szybkich kliknięciach.

### 8.3 Multi-Shot (Power-Up)

Power-up Multi-Shot dodaje boczne pociski wystrzelane jednocześnie z głównym. Kąt rozproszenia jest obliczany dynamicznie:

```csharp
angle = (i - (totalProjectiles - 1) / 2f) * 15f;
```

Efekt trwa określony czas (`duration`), po którym liczba dodatkowych pocisków wraca do zera.

### 8.4 Tarcza (Power-Up)

Power-up Shield aktywuje tarczę absorbującą jedno trafienie. Wizualnie tarcza jest wyświetlana jako osobny sprite (wariant niebieski lub pomarańczowy zależnie od statku) z włączonym `CircleCollider2D`.

### 8.5 Obrażenia i Śmierć

`DamageDealer` na prefabie pocisku przechowuje wartość obrażeń. Przy trafieniu `Health.TakeDamage()` odejmuje HP. Gdy HP spada do zera:
- **Gracz** → `LevelManager.LoadGameOver()` z 2-sekundowym opóźnieniem
- **Wróg** → `ScoreKeeper.AddScore()` + `PowerUpSpawner.SpawnPowerUp()`

---

## 9. Wrogowie i Fale

### 9.1 Konfiguracja Fal (WaveConfigSO)

Każda fala wrogów jest opisana przez plik ScriptableObject `WaveConfigSO` zawierający:
- tablicę prefabów wrogów do spawnu
- referencję do trasy (prefab z waypointami)
- prędkość poruszania wrogów
- czas i wariancję między spawnami

### 9.2 Spawning

`EnemySpawner.cs` iteruje po tablicy `waveConfigs` i dla każdej fali instancjonuje wrogów z odstępami. Obsługuje opcjonalne zapętlanie fal (`isLooping`). Pozycje spawnu są skalowane do rozdzielczości ekranu:

```csharp
pathScaleX = (cam.orthographicSize * cam.aspect) / designHalfWidth;
spawnPos.x *= pathScaleX;
```

Dzięki temu trasy wrogów wyglądają poprawnie niezależnie od rozdzielczości ekranu.

### 9.3 Ruch Wrogów (PathFinding)

Każdy wróg po spawnie pobiera z `EnemySpawner` aktualną konfigurację fali i przesuwa się sekwencyjnie przez kolejne waypoints metodą `Vector2.MoveTowards()`. Po dotarciu do ostatniego waypointa obiekt wroga jest niszczony.

---

## 10. Power-Upy

### 10.1 System Dropu

`PowerUpSpawner` implementuje probabilistyczny system dropu:

1. Gdy wróg ginie — wywołuje `SpawnPowerUp()`
2. Jeśli na ekranie jest mniej niż 3 power-upy — próba dropu
3. Losowany jest `dropRate` z zakresu `[minDropRate, 1.0]`
4. Jeśli `dropRate >= 0.5` → drop power-upa, losowy typ z listy
5. Jeśli `dropRate < 0.5` → brak dropu, `minDropRate += 0.1` (rośnie szansa przy kolejnym wrogu)
6. Gdy ekran jest pełny (3 power-upy) — aktywowany jest cooldown 10 sekund

Każdy power-up istnieje na ekranie maksymalnie 8 sekund, po czym jest automatycznie niszczony.

### 10.2 Typy Power-Upów

| Power-Up | Efekt |
|---|---|
| `HealthPowerUp` | Leczy gracza o `healAmount` HP (do maksimum) |
| `ShieldPowerUp` | Aktywuje tarczę absorbującą jedno trafienie |
| `MultiShotPowerUp` | Przez określony czas dodaje boczne pociski do każdego strzału |

---

## 11. UI i HUD

### 11.1 Menu Główne (MainMenu)

Przyciski: **Play** (przechodzi do `CharSelectScene`) i **Quit** (zamyka aplikację).

### 11.2 Wybór Postaci (CharSelectScene)

Dwa przyciski — **Blue Ship** i **Orange Ship**. Kliknięcie zapisuje wybór w `CharSelectManager` i ładuje `GameScene`.

### 11.3 HUD (GameScene)

`UIUpdater.cs` aktualizuje co klatkę:
- **Pasek zdrowia** — `Slider` + `Image.fillAmount` proporcjonalnie do `HP / maxHP`
- **Wynik** — `TextMeshProUGUI` wyświetlający score w formacie `000000`
- **Schemat kolorystyczny** — automatycznie dostosowany do wybranego statku

`HPBarManager.cs` ustawia właściwy sprite paska zdrowia przy starcie sceny.

### 11.4 Menu Ulepszeń (Upgrade Menu)

Dostępne klawiszem **U** podczas gry — pauzuje czas (`Time.timeScale = 0`).

### 11.5 Menu Audio

Dostępne klawiszem **M** podczas gry — pauzuje czas (`Time.timeScale = 0`). Zawiera trzy suwaki głośności.

### 11.6 Wzajemne Wykluczanie Menu

Otwarcie jednego menu automatycznie zamyka drugie. `UIPointerBlocker` blokuje strzelanie gdy kursor gracza znajduje się nad elementem UI.

### 11.7 Ekran Game Over

`UIGameOver.cs` pobiera wynik z `ScoreKeeper` i wyświetla go jako `"Final Score:\nXXX"`. Przyciski: **Play Again** i **Main Menu**.

---

## 12. System Audio

### 12.1 Architektura

`AudioManager` (Singleton) zarządza całym dźwiękiem przez Unity **AudioMixer** z trzema oddzielnymi grupami:

```
AudioMixer
├── Master (masterVolumeParam)
│   ├── Music (musicVolumeParam)
│   └── SFX   (sfxVolumeParam)
```

Konwersja wartości suwaka (0–1) na decybele odbywa się logarytmicznie:
```csharp
float dbValue = sliderValue > 0.0001f ? Mathf.Log10(sliderValue) * 20 : -80f;
```

### 12.2 Persystencja Ustawień

Głośność jest zapisywana przez `PlayerPrefs` i odczytywana przy każdym starcie. Domyślna głośność wszystkich kanałów: **75%**.

### 12.3 Efekty Dźwiękowe (SFX)

| Klip | Zdarzenie |
|---|---|
| `shootingClip` | Każdy wystrzelony pocisk |
| `takeDamageClip` | Trafienie pociskiem (gracz lub wróg) |
| `chargeUpClip` | Aktywny stan ładowania (odtwarzany co klatkę) |
| `chargingShotClip` | Oddanie naładowanego strzału |

### 12.4 Muzyka

Projekt zawiera **5 tracków** z pakietu *Juhani Junkala Retro Game Music Pack*:

| Plik | Użycie |
|---|---|
| `Title Screen.wav` | Menu główne |
| `Level 1.wav` | Pierwszy etap rozgrywki |
| `Level 2.wav` | Drugi etap rozgrywki |
| `Level 3.wav` | Trzeci etap rozgrywki |
| `Ending.wav` | Ekran Game Over |

### 12.5 Regulacja Głośności w Trakcie Gry

Menu audio (klawisz **M**) otwiera panel z trzema suwakami — Master, Music, SFX. Każdy suwak jest podłączony przez `onValueChanged` do `AudioManager.SetVolume()`, który natychmiast aktualizuje AudioMixer i zapisuje wartość do `PlayerPrefs`.

---

## 13. System Animacji

### 13.1 Kontrolery Animatora

Projekt zawiera **8 kontrolerów** Animatora przypisanych do różnych obiektów:

| Kontroler | Obiekt | Parametry |
|---|---|---|
| `Player.controller` | Statek gracza (Blue) | `isCharging`, `isShooting`, `isNormalShooting` |
| `OrangeShooting.controller` | Statek gracza (Orange) | `isNormalShooting` |
| `OrangeChargeShooting.controller` | Statek gracza (Orange) | `isCharging`, `isShooting` |
| `Shooting.controller` | Efekt strzału (Blue) | `isNormalShooting` |
| `ChargeShooting.controller` | Efekt ładowania (Blue) | `isCharging` |
| `ChargedShootingAnimation.controller` | Efekt naładowanego strzału | `isShooting` |
| `EnemyShooting.controller` | Wróg | automatyczne |
| `BlueShield.controller` / `OrangeShield.controller` | Tarcza | automatyczne |

### 13.2 Menedżery Animacji

Odpowiedzialność za sterowanie Animatorem jest podzielona na cztery dedykowane klasy:

| Klasa | Parametr | Wywołujący |
|---|---|---|
| `NormalShootingAnimationManager` | `isNormalShooting` (bool) | `Shooter.cs` |
| `ChargingAnimationManager` | `isCharging` (bool) | `PlayerController.cs` |
| `ChargedShootingAnimationManager` | `isShooting` (bool) | `PlayerController.cs` |
| `ShieldAnimationManager` | brak Animatora — toggle `SpriteRenderer` + `CircleCollider2D` | `Health.cs` |

### 13.3 Animacja Paska Ładowania

`ChargeBarManager` nie korzysta z komponentu Animator — animacja paska jest realizowana przez ręczną zmianę sprite'a co klatkę:

```csharp
float progress = Mathf.Clamp01(chargeTimer / effectiveChargeTime);
int frameIndex = Mathf.Clamp(
    Mathf.FloorToInt(progress * (currentFrames.Length - 1)),
    0, currentFrames.Length - 1
);
spriteRenderer.sprite = currentFrames[frameIndex];
```

Zestaw klatek (`blueFrames` lub `orangeFrames`) jest wybierany na podstawie wybranego statku.

---

## 14. Autorska Mechanika Rozszerzona — System Ulepszeń

### 14.1 Opis

**Upgrade Menu** to in-game system progresji umożliwiający graczowi trwałe wzmocnienie statku za zdobyte punkty bez konieczności przerywania rozgrywki. Menu otwiera się klawiszem **U** i automatycznie pauzuje czas gry.

### 14.2 Ścieżki Ulepszeń

| Ulepszenie | Efekt na poziom | Koszt bazowy | Maks. poziom |
|---|---|---|---|
| **DAMAGE** | +25% obrażeń wszystkich pocisków | 50 pkt | 5 |
| **RELOAD** | −15% czasu ładowania Power Shota | 75 pkt | 5 |
| **HEALTH** | +10% maksymalnego HP (kompozytowo) | 100 pkt | 5 |

Koszt każdego kolejnego poziomu rośnie liniowo: `baseCost × (level + 1)`. Przykładowo ulepszenie DAMAGE na poziomie 3 kosztuje `50 × 3 = 150 pkt`.

### 14.3 Architektura Systemu

Odpowiedzialność jest ściśle podzielona na dwie klasy zgodnie z zasadą SRP:

**`UpgradeManager`** — logika biznesowa:
- przechowuje aktualne poziomy ulepszeń
- oblicza koszty (`GetCost()`)
- realizuje zakup (`TryBuy()`) — atomowo: albo wydaje punkty i awansuje poziom, albo nic
- aplikuje efekty (mnoży obrażenia, zmniejsza czas ładowania, zwiększa maxHP)
- emituje event `OnUpgradeChanged` po każdej zmianie

**`UpgradeMenuController`** — warstwa prezentacji:
- nasłuchuje na `OnUpgradeChanged` i odświeża etykiety
- koloruje przyciski: zielony (stać), czerwony (za mało punktów), szary (max poziom)
- blokuje nieinteraktywne przyciski (`btn.interactable = false`)
- obsługuje toggle panelu i wzajemne wykluczanie z menu audio

### 14.4 Integracja z Innymi Systemami

Efekty ulepszeń są odpytywane bezpośrednio przez inne skrypty za pomocą metod `UpgradeManager`:

| Skrypt | Metoda | Zastosowanie |
|---|---|---|
| `Shooter.cs` | `GetDamageMultiplier()` | Mnoży `damage` każdego pocisku gracza przy instancjonowaniu |
| `ChargeBarManager.cs` | `GetChargeTimeMultiplier()` | Skaluje `effectiveChargeTime` przy starcie ładowania |
| `Health.cs` | `IncreaseMaxHealth(amount)` | Bezpośrednio zwiększa `maxHealth` i `health` o obliczoną wartość |

```csharp
// Shooter.cs — aplikacja mnożnika obrażeń
DamageDealer damageDealer = projectile.GetComponent<DamageDealer>();
damageDealer.MultiplyDamage(upgradeManager.GetDamageMultiplier());

// ChargeBarManager.cs — skrócenie czasu ładowania
float multiplier = upgradeManager.GetChargeTimeMultiplier();
effectiveChargeTime = maxChargeTime * multiplier;   // max −75% na poziomie 5
```

### 14.5 Kolorowanie Przycisków

```csharp
static readonly Color colAffordable   = new Color(0.10f, 0.38f, 0.18f);  // ciemny zielony
static readonly Color colUnaffordable = new Color(0.38f, 0.10f, 0.10f);  // ciemny czerwony
static readonly Color colMaxed        = new Color(0.22f, 0.22f, 0.22f);  // szary
```

Kolory są aktualizowane przy każdym wywołaniu `RefreshLabels()` — po zakupie i przy otwieraniu panelu.

---

## 15. Dodatkowe Systemy

### 15.1 Efekt Drżenia Kamery (CameraShake)

Przy każdym trafieniu gracza kamera losowo przesuwa się przez `shakeDuration` sekund z siłą `shakeMagnitude`, a następnie płynnie wraca do pozycji wyjściowej. Efekt jest realizowany koroutyną.

### 15.2 Przewijanie Tła (BackgroundScroller)

Tekstura tła jest przesuwana co klatkę przez modyfikację `material.mainTextureOffset`. Tworzy to wrażenie lotu przez kosmos bez faktycznego przemieszczania obiektów sceny. Prędkości X i Y są niezależnie konfigurowalne.

### 15.3 Efekty Cząsteczkowe

Przy każdym trafieniu `Health.PlayHitParticles()` instancjonuje prefab `ParticleSystem` w miejscu kolizji. Czas życia efektu jest obliczany dynamicznie: `duration + startLifetime`. Po tym czasie obiekt jest niszczony.
