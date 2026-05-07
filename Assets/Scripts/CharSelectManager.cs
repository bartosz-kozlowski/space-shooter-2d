using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton zarządzający wyborem statku gracza w scenie CharSelect.
/// Wybrany statek persystuje między scenami do momentu powrotu do menu.
/// </summary>
public class CharSelectManager : MonoBehaviour
{
    enum Ship
    {
        Blue = 0,
        Orange = 1
    }

    int currentShipIndex;           // Indeks wybranego statku
    static CharSelectManager instance;

    /// <summary>
    /// Inicjalizuje Singleton — wykonuje ManageSingleton() aby zagwarantować jedną instancję.
    /// </summary>
    void Awake()
    {
        ManageSingleton();
    }

    /// <summary>
    /// Implementacja wzorca Singleton — zapewnia istnienie tylko jednej instancji.
    /// </summary>
    void ManageSingleton()
    {
        if (instance != null)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// Zwraca indeks aktualnie wybranego statku (0=Blue, 1=Orange).
    /// </summary>
    public int GetCurrentShipIndex()
    {
        return currentShipIndex;
    }

    /// <summary>
    /// Ustawia statek na niebieski (indeks 0).
    /// </summary>
    public void SetBluePlayer()
    {
        currentShipIndex = (int)Ship.Blue;
    }

    /// <summary>
    /// Ustawia statek na pomarańczowy (indeks 1).
    /// </summary>
    public void SetOrangePlayer()
    {
        currentShipIndex = (int)Ship.Orange;
    }

    /// <summary>
    /// Resetuje wybór statku na wartość domyślną (-1) przy powrocie do menu.
    /// </summary>
    public void ResetCurrentPlayer()
    {
        currentShipIndex = -1;
    }
}
