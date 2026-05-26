using UnityEngine;

/// <summary>
/// Singleton zarządzający wynikiem gracza podczas rozgrywki.
/// Wynik persystuje między scenami (DontDestroyOnLoad).
/// </summary>
public class ScoreKeeper : MonoBehaviour
{
    int score = 0;

    static ScoreKeeper instance;

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
    /// Zwraca bieżący wynik gracza.
    /// </summary>
    public int GetScore()
    {
        return score;
    }

    /// <summary>
    /// Dodaje punkty do wyniku gracza. Wynik nie może być ujemny.
    /// </summary>
    public void AddScore(int scoreToAdd)
    {
        score += scoreToAdd;
        score = Mathf.Clamp(score, 0, int.MaxValue);
        Debug.Log($"Score: {score}");
    }

    /// <summary>
    /// Resetuje wynik na 0 przy starcie nowej gry.
    /// </summary>
    public void ResetScore()
    {
        score = 0;
    }

    /// <summary>
    /// Próbuje wydać punkty — zwraca true tylko gdy gracz miał dość punktów.
    /// Operacja jest atomowa: albo wydajemy całość, albo nic.
    /// </summary>
    public bool SpendScore(int amount)
    {
        if (amount <= 0) return false;
        if (score < amount) return false;
        score -= amount;
        return true;
    }
}
