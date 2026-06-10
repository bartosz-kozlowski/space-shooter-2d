using UnityEngine;

/// <summary>
/// Power-up: Tymczasowo zwiększa liczbę pocisków wystrzelonych naraz.
/// Efekt trwa określony czas.
/// </summary>
[CreateAssetMenu(fileName = "MultiShotPowerUp", menuName = "MultiShotPowerUp")]
public class MultiShotPowerUp : PowerUpSO
{
    [SerializeField] int projectileAmount;  // Liczba dodatkowych pocisków
    [SerializeField] float duration;        // Czas trwania efektu

    /// <summary>
    /// Aktywuje multi-shot na określony czas - gracz strzela wieloma pociskami.
    /// </summary>
    public override void Apply(GameObject player)
    {
        Shooter shooter = player.GetComponentInChildren<Shooter>();
        shooter.CallMultiShotCoroutine(projectileAmount, duration);
    }
}
