using UnityEngine;

/// <summary>
/// Animuje pasek ładowania strzału poprzez zmianę sprite'ów.
/// Wyświetla różne klatki animacji w zależności od wybranego statku i postępu ładowania.
/// </summary>
public class ChargeBarManager : MonoBehaviour
{
    [SerializeField] Sprite[][] chargeBarFrames;   // Nieużywane — zamiast tego używamy blueFrames/orangeFrames
    [SerializeField] Sprite[] blueFrames;          // Klatki animacji dla niebieskiego statku
    [SerializeField] Sprite[] orangeFrames;        // Klatki animacji dla pomarańczowego statku
    [SerializeField] float maxChargeTime = 1.5f;   // Maksymalny czas ładowania

    SpriteRenderer spriteRenderer;
    CharSelectManager charSelectManager;
    Sprite[] currentFrames;     // Bieżące klatki zależne od wybranego statku
    bool isCharging;            // Czy trwa ładowanie
    float chargeTimer;          // Zegar postępu ładowania

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
        Sprite[][] allFrames = { blueFrames, orangeFrames };
        int index = charSelectManager.GetCurrentShipIndex();
        currentFrames = allFrames[index];
        spriteRenderer.enabled = false;
    }

    /// <summary>
    /// Aktualizuje sprite paska ładowania co klatkę na podstawie postępu.
    /// </summary>
    void Update()
    {
        if (!isCharging) return;

        chargeTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(chargeTimer / maxChargeTime);
        int frameIndex = Mathf.Min(Mathf.FloorToInt(progress * currentFrames.Length), currentFrames.Length - 1);
        spriteRenderer.sprite = currentFrames[frameIndex];
    }

    /// <summary>
    /// Uruchamia animację paska — wyświetla pasek i resetuje timer.
    /// </summary>
    public void PlayChargeBarAnimation()
    {
        isCharging = true;
        chargeTimer = 0f;
        spriteRenderer.enabled = true;
    }

    /// <summary>
    /// Zatrzymuje animację paska — ukrywa pasek.
    /// </summary>
    public void StopChargeBarAnimation()
    {
        isCharging = false;
        spriteRenderer.enabled = false;
    }
}
