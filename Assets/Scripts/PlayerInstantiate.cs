using UnityEngine;

/// <summary>
/// Factory pattern — tworzy instancję wybranego statku gracza na podstawie wyboru z CharSelectManager.
/// Jest przykładem wzorca Factory — wybór typu obiektu odbywa się podczas runtime'u.
/// </summary>
public class PlayerInstantiate : MonoBehaviour
{
    [SerializeField] GameObject[] shipPrefabs;  // Prefaby dostępnych statków (indeks: 0=Blue, 1=Orange)

    CharSelectManager charSelectManager;
    int shipIndex;

    /// <summary>
    /// Pobiera wybrany indeks statku z Singletonu i instancjonuje odpowiedni prefab.
    /// </summary>
    void Awake()
    {
        charSelectManager = FindFirstObjectByType<CharSelectManager>();
        shipIndex = charSelectManager.GetCurrentShipIndex();
        Instantiate(shipPrefabs[shipIndex]);
    }
}
