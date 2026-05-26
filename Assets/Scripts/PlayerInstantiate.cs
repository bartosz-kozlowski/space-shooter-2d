using UnityEngine;

/// <summary>
/// Factory Pattern — enkapsuluje logikę tworzenia statku gracza.
/// Kod zewnętrzny wywołuje CreateShip() nie wiedząc nic o prefabach ani konfiguracji.
/// </summary>
public class PlayerInstantiate : MonoBehaviour
{
    [SerializeField] GameObject[] shipPrefabs;  // Prefaby dostępnych statków (indeks: 0=Blue, 1=Orange)

    void Awake()
    {
        CharSelectManager charSelectManager = FindFirstObjectByType<CharSelectManager>();
        CreateShip(charSelectManager.GetCurrentShipIndex());
    }

    /// <summary>
    /// Fabryka statków — jedyne miejsce w kodzie które zna prefaby i decyduje który stworzyć.
    /// </summary>
    GameObject CreateShip(int shipIndex)
    {
        if (shipIndex < 0 || shipIndex >= shipPrefabs.Length)
            throw new System.ArgumentOutOfRangeException(nameof(shipIndex), "Nieprawidłowy indeks statku.");

        return Instantiate(shipPrefabs[shipIndex]);
    }
}
