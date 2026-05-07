using UnityEngine;

/// <summary>
/// Power-up: Leczenie gracza o określoną liczbę HP.
/// Nie może przekroczyć maksymalnego zdrowia.
/// </summary>
[CreateAssetMenu(fileName = "HealthPowerUp", menuName = "HealthPowerUp")]
public class HealthPowerUp : PowerUpSO
{
    [SerializeField] int healAmount = 50; // Liczba HP do wyleczenia

    /// <summary>
    /// Leczy gracza o określoną liczbę punktów zdrowia.
    /// </summary>
    public override void Apply(GameObject player)
    {
        Health health = player.GetComponent<Health>();
        health.Heal(healAmount);
    }
}
