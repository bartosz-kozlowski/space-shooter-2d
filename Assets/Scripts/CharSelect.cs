using UnityEngine;

/// <summary>
/// Obsługuje wybór statku gracza w scenie CharSelect.
/// Metody są wywoływane przez przyciski UI - zapisują wybór i przechodzą do gry.
/// </summary>
public class CharSelect : MonoBehaviour
{
    CharSelectManager charSelectManager;
    LevelManager levelManager;

    /// <summary>
    /// Znajduje Singletony niezbędne do wyboru statku i ładowania gry.
    /// </summary>
    void Awake()
    {
        charSelectManager = FindFirstObjectByType<CharSelectManager>();
        levelManager = FindFirstObjectByType<LevelManager>();
    }

    /// <summary>
    /// Ustawia niebieski statek i uruchamia grę.
    /// </summary>
    public void ChooseBluePlayer()
    {
        charSelectManager.SetBluePlayer();
        levelManager.LoadGame();
    }

    /// <summary>
    /// Ustawia pomarańczowy statek i uruchamia grę.
    /// </summary>
    public void ChooseOrangePlayer()
    {
        charSelectManager.SetOrangePlayer();
        levelManager.LoadGame();
    }
}
