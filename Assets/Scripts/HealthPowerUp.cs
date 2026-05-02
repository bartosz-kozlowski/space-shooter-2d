using UnityEngine;

[CreateAssetMenu(fileName = "HealthPowerUp", menuName = "HealthPowerUp")]
public class HealthPowerUp : PowerUpSO
{
    [SerializeField] int healAmount = 50;

    public override void Apply(GameObject player)
    {
        Health health = player.GetComponent<Health>();
        health.Heal(healAmount);
    }
}
