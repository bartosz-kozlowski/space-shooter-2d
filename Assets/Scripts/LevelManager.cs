using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Zarządza ładowaniem scen gry - przechodzi między scenami (GameScene, GameOver, MainMenu, CharSelectScene).
/// Resetuje wynik przy starcie nowej gry, obsługuje opóźnienie dla animacji śmierci gracza.
/// </summary>
public class LevelManager : MonoBehaviour
{
    ScoreKeeper scoreKeeper;
    CharSelectManager charSelectManager;

    /// <summary>
    /// Znajduje Singletony ScoreKeeper i CharSelectManager na scenie.
    /// </summary>
    void Awake()
    {
        scoreKeeper = FindFirstObjectByType<ScoreKeeper>();
        charSelectManager = FindFirstObjectByType<CharSelectManager>();
    }

    /// <summary>
    /// Resetuje wynik i ładuje scenę GameScene - wywołane przy wyborze statku.
    /// </summary>
    public void LoadGame()
    {
        scoreKeeper.ResetScore();
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// Ładuje scenę GameOver z 2-sekundowym opóźnieniem na animację śmierci gracza.
    /// </summary>
    public void LoadGameOver()
    {
        StartCoroutine(WaitAndLoad("GameOver", 2f));
    }

    /// <summary>
    /// Resetuje wybór statku i ładuje scenę MainMenu.
    /// </summary>
    public void LoadMainMenu()
    {
        charSelectManager.ResetCurrentPlayer();
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Ładuje scenę wyboru statku (CharSelectScene).
    /// </summary>
    public void LoadChooseCharacter()
    {
        SceneManager.LoadScene("CharSelectScene");
    }

    /// <summary>
    /// Coroutine: czeka zadany czas, następnie ładuje scenę - używane do opóźnień animacyjnych.
    /// </summary>
    IEnumerator WaitAndLoad(string sceneName, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        SceneManager.LoadScene(sceneName);
    }
}
