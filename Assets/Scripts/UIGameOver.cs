using TMPro;
using UnityEngine;

/// <summary>
/// Wyświetla finalny wynik gracza na ekranie Game Over.
/// Pobiera wynik z Singletonu ScoreKeeper.
/// </summary>
public class UIGameOver : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText; // Tekst wyświetlający wynik

    ScoreKeeper scoreKeeper;

    /// <summary>
    /// Znajduje Singleton ScoreKeeper na scenie.
    /// </summary>
    void Awake()
    {
        scoreKeeper = FindFirstObjectByType<ScoreKeeper>();
    }

    /// <summary>
    /// Wyświetla finalny wynik gracza w formacie "Final Score: XXX".
    /// </summary>
    void Start()
    {
        scoreText.text = "Final Score:\n" + scoreKeeper.GetScore();
    }
}
