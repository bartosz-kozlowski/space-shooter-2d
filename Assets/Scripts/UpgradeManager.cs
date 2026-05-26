using System;
using UnityEngine;

/// <summary>
/// Zarządza systemem progresji statku — gracz wymienia punkty za wzmocnienia
/// (obrażenia, szybsze ładowanie Power Shota, większe maksymalne zdrowie).
/// Umieść ten komponent na GameObjectcie w GameScene i przypisz referencje w Inspectorze.
/// Resetuje się automatycznie przy każdym załadowaniu sceny (nie DontDestroyOnLoad).
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    public enum UpgradeType { Damage, Reload, Health }

    public const int MaxLevel = 5;

    public const int DamageBaseCost = 50;
    public const int ReloadBaseCost = 75;
    public const int HealthBaseCost = 100;

    public const float DamagePerLevel = 0.25f;
    public const float ReloadPerLevel = 0.15f;
    public const float HealthPerLevel = 0.10f;  // +10% maxHealth na poziom (kompozytowo)

    int damageLevel;
    int reloadLevel;
    int healthLevel;

    public event Action OnUpgradeChanged;

    ScoreKeeper scoreKeeper;
    Health playerHealth;

    void Start()
    {
        scoreKeeper = FindFirstObjectByType<ScoreKeeper>();
    }

    Health GetPlayerHealth()
    {
        if (playerHealth != null) return playerHealth;
        foreach (Health h in FindObjectsByType<Health>(FindObjectsSortMode.None))
        {
            if (h.GetIsPlayer()) { playerHealth = h; break; }
        }
        return playerHealth;
    }

    public int GetLevel(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Damage: return damageLevel;
            case UpgradeType.Reload: return reloadLevel;
            case UpgradeType.Health: return healthLevel;
            default: return 0;
        }
    }

    /// <summary>
    /// Koszt następnego poziomu — rośnie liniowo (base × (level+1)).
    /// Zwraca -1 gdy osiągnięto MaxLevel.
    /// </summary>
    public int GetCost(UpgradeType type)
    {
        int level = GetLevel(type);
        if (level >= MaxLevel) return -1;

        int baseCost;
        switch (type)
        {
            case UpgradeType.Damage: baseCost = DamageBaseCost; break;
            case UpgradeType.Reload: baseCost = ReloadBaseCost; break;
            case UpgradeType.Health: baseCost = HealthBaseCost; break;
            default: return -1;
        }
        return baseCost * (level + 1);
    }

    public bool TryBuy(UpgradeType type)
    {
        int level = GetLevel(type);
        if (level >= MaxLevel) return false;
        if (scoreKeeper == null) scoreKeeper = FindFirstObjectByType<ScoreKeeper>();
        if (scoreKeeper == null) return false;
        if (!scoreKeeper.SpendScore(GetCost(type))) return false;

        ApplyLevel(type, level + 1);
        return true;
    }

    void ApplyLevel(UpgradeType type, int newLevel)
    {
        switch (type)
        {
            case UpgradeType.Damage: damageLevel = newLevel; break;
            case UpgradeType.Reload: reloadLevel = newLevel; break;
            case UpgradeType.Health:
                healthLevel = newLevel;
                Health hp = GetPlayerHealth();
                if (hp != null)
                {
                    int increase = Mathf.Max(1, Mathf.RoundToInt(hp.GetMaxHealth() * HealthPerLevel));
                    hp.IncreaseMaxHealth(increase);
                }
                break;
        }
        OnUpgradeChanged?.Invoke();
    }

    public float GetDamageMultiplier()     => 1f + damageLevel * DamagePerLevel;
    public float GetChargeTimeMultiplier() => Mathf.Max(0.1f, 1f - reloadLevel * ReloadPerLevel);
}
