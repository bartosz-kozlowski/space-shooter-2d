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

    /// <summary>
    /// Skaluje obrażenia mnożnikiem - używane do aplikacji ulepszenia DAMAGE
    /// na pocisk gracza w momencie jego instancjowania.
    /// </summary>
    public void MultiplyDamage(float multiplier)
    {
        damage = Mathf.Max(1, Mathf.RoundToInt(damage * multiplier));
    }
}
