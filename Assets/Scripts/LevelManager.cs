using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    ScoreKeeper scoreKeeper;
    CharSelectManager charSelectManager;

    void Awake()
    {
        scoreKeeper = FindFirstObjectByType<ScoreKeeper>();
        charSelectManager = FindFirstObjectByType<CharSelectManager>();
    }

    public void LoadGame()
    {
        scoreKeeper.ResetScore();
        SceneManager.LoadScene("GameScene");
    }

    public void LoadGameOver()
    {
        StartCoroutine(WaitAndLoad("GameOver", 2f));
    }

    public void LoadMainMenu()
    {
        charSelectManager.ResetCurrentPlayer();
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadChooseCharacter()
    {
        SceneManager.LoadScene("CharSelectScene");
    }

    IEnumerator WaitAndLoad(string sceneName, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        SceneManager.LoadScene(sceneName);
    }
}
