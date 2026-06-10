using UnityEngine;

/// <summary>
/// Animuje pasek ładowania strzału poprzez zmianę sprite'ów.
/// Wyświetla różne klatki animacji w zależności od wybranego statku i postępu ładowania.
/// </summary>
public class ChargeBarManager : MonoBehaviour
{
    [SerializeField] Sprite[][] chargeBarFrames;   // Nieużywane - zamiast tego używamy blueFrames/orangeFrames
    [SerializeField] Sprite[] blueFrames;           // Klatki animacji dla niebieskiego statku
    [SerializeField] Sprite[] orangeFrames;         // Klatki animacji dla pomarańczowego statku
    [SerializeField] float blueMaxChargeTime  = 6f; // Bazowy czas ładowania dla Blue
    [SerializeField] float orangeMaxChargeTime = 3.5f; // Bazowy czas ładowania dla Orange
    float maxChargeTime;
    public float MaxChargeTime => maxChargeTime;

    public event System.Action OnChargeFull;  // Odpala się dokładnie gdy pasek osiągnie 100%

    SpriteRenderer spriteRenderer;
    CharSelectManager charSelectManager;
    UpgradeManager upgradeManager;  // Cache do mnożnika czasu ładowania (lazy lookup)
    Sprite[] currentFrames;     // Bieżące klatki zależne od wybranego statku
    bool isCharging;            // Czy trwa ładowanie
    bool chargeFullFired;       // Czy event OnChargeFull już wysłano w tej sesji ładowania
    float chargeTimer;          // Zegar postępu ładowania
    float effectiveChargeTime;  // Efektywny czas ładowania po uwzględnieniu ulepszenia RELOAD

    /// <summary>
    /// Znajduje komponenty SpriteRenderer i CharSelectManager.
    /// </summary>
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        charSelectManager = FindFirstObjectByType<CharSelectManager>();
    }

    /// <summary>
    /// Ustawia klatki animacji na podstawie wybranego statku.
    /// </summary>
    void Start()
    {
        int index = charSelectManager.GetCurrentShipIndex();
        Sprite[][] allFrames = { blueFrames, orangeFrames };
        float[] chargeTimes = { blueMaxChargeTime, orangeMaxChargeTime };
        currentFrames = allFrames[index];
        maxChargeTime  = chargeTimes[index];
        spriteRenderer.enabled = false;
    }

    /// <summary>
    /// Aktualizuje sprite paska ładowania co klatkę na podstawie postępu.
    /// </summary>
    void Update()
    {
        if (!isCharging) return;

        chargeTimer += Time.deltaTime;

        if (!chargeFullFired && chargeTimer >= effectiveChargeTime)
        {
            chargeFullFired = true;
            OnChargeFull?.Invoke();
        }

        float progress = Mathf.Clamp01(chargeTimer / effectiveChargeTime);
        // Mapowanie progress→frameIndex: ostatnia klatka tylko przy progress=1.0
        // (z (Length) zamiast (Length-1) ostatnia klatka pokazywała się już przy 90% → desync z chargeTimer)
        int frameIndex = Mathf.Clamp(Mathf.FloorToInt(progress * (currentFrames.Length - 1)), 0, currentFrames.Length - 1);
        spriteRenderer.sprite = currentFrames[frameIndex];
    }

    /// <summary>
    /// Uruchamia animację paska - wyświetla pasek i resetuje timer.
    /// Czas ładowania jest skalowany mnożnikiem z UpgradeManager (ulepszenie RELOAD).
    /// </summary>
    public void PlayChargeBarAnimation()
    {
        isCharging = true;
        chargeTimer = 0f;
        spriteRenderer.enabled = true;

        if (upgradeManager == null) upgradeManager = FindFirstObjectByType<UpgradeManager>();
        float multiplier = upgradeManager != null ? upgradeManager.GetChargeTimeMultiplier() : 1f;
        effectiveChargeTime = maxChargeTime * multiplier;
    }

    /// <summary>
    /// Zatrzymuje animację paska - ukrywa pasek.
    /// </summary>
    public void StopChargeBarAnimation()
    {
        isCharging = false;
        chargeFullFired = false;
        spriteRenderer.enabled = false;
    }
}
