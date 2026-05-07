using UnityEngine;

/// <summary>
/// Abstrakcyjna klasa bazowa dla wszystkich typów power-upów.
/// Definiuje interfejs dla zastosowania efektu power-upa na graczu.
/// </summary>
public abstract class PowerUpSO : ScriptableObject
{
    [SerializeField] string powerUpName; // Nazwa do debugowania

    /// <summary>
    /// Zastosuj efekt power-upa na graczu (implementacja w podklasach).
    /// </summary>
    public abstract void Apply(GameObject player);
}
