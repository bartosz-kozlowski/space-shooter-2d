using UnityEngine;

/// <summary>
/// Komponent pocisku/pocisku określający ilość obrażeń zadanych przy trafieniu.
/// Po trafieniu w cel pocisk jest automatycznie niszczony.
/// </summary>
public class DamageDealer : MonoBehaviour
{
    [SerializeField] int damage = 10; // Ilość obrażeń zadawanych przy trafieniu

    /// <summary>
    /// Zwraca wartość obrażeń tego pocisku.
    /// </summary>
    public int GetDamage()
    {
        return damage;
    }

    /// <summary>
    /// Niszczy pocisk po trafieniu w cel.
    /// </summary>
    public void Hit()
    {
        Destroy(gameObject);
    }
}
