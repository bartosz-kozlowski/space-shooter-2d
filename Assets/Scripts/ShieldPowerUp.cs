using UnityEngine;

/// <summary>
/// Power-up: Aktywuje tarczę ochronną na graczu.
/// Tarcza absorbuje jedno trafienie, następnie się dezaktywuje.
/// </summary>
[CreateAssetMenu(fileName = "ShieldPowerUp", menuName = "ShieldPowerUp")]
public class ShieldPowerUp : PowerUpSO
{
    /// <summary>
    /// Aktywuje tarczę ochronną na graczu.
    /// </summary>
    public override void Apply(GameObject player)
    {
        Health health = player.GetComponent<Health>();
        health.ActivateShield();
    }
}
