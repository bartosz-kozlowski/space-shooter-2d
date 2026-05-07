using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Zarządza wyświetlaniem interfejsu gracza (HUD) podczas rozgrywki.
/// Aktualizuje pasek zdrowia, wynik punktowy i kolory UI w zależności od wybranej postaci.
/// </summary>
public class UIUpdater : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] Slider healthSlider;
    [SerializeField] Image healthFillImage;

    [Header("Score")]
    [SerializeField] TextMeshProUGUI scoreText;

    ScoreKeeper scoreKeeper;
    CharSelectManager charSelectManager;
    Health playerHeath;
    Health[] healthList;
    float maxHealthPlayer;

    static readonly Color blueTop = new Color(0x49 / 255f, 0xFF / 255f, 0xF3 / 255f, 1f);
    static readonly Color blueBottom = new Color(0x6B / 255f, 0xAB / 255f, 0xFF / 255f, 1f);
    static readonly Color orangeColor = new Color(1f, 0.5f, 0f, 1f);

    /// <summary>
    /// Inicjalizuje HUD — znajduje gracza, singletonów i ustawia wstępne wartości.
    /// </summary>
    void Start()
    {
        InitializePlayerHealth();
        scoreKeeper = FindFirstObjectByType<ScoreKeeper>();
        charSelectManager = FindFirstObjectByType<CharSelectManager>();

        maxHealthPlayer = playerHeath.GetHealth();
        healthFillImage.fillAmount = playerHeath.GetHealth() / maxHealthPlayer;

        ApplyScoreTextColor();
    }

    /// <summary>
    /// Wyszukuje gracza wśród wszystkich Health komponentów na scenie.
    /// </summary>
    void InitializePlayerHealth()
    {
        healthList = FindObjectsByType<Health>(FindObjectsSortMode.None);

        foreach(Health health in healthList)
        {
            if (health.GetIsPlayer() == true)
            {
                playerHeath = health;
                break;
            }
        }
    }

    /// <summary>
    /// Ustawia kolor tekstu wyniku w zależności od wybranego statku gracza.
    /// Niebieski statek — gradient, zielony statek — pomarańczowy.
    /// </summary>
    void ApplyScoreTextColor()
    {
        if (charSelectManager.GetCurrentShipIndex() == 0)
        {
            scoreText.enableVertexGradient = true;
            scoreText.colorGradient = new VertexGradient(blueTop, blueTop, blueBottom, blueBottom);
        }
        else
        {
            scoreText.enableVertexGradient = false;
            scoreText.color = orangeColor;
        }
    }

    /// <summary>
    /// Aktualizuje HUD każdej klatce: wynik i pasek zdrowia gracza.
    /// </summary>
    void Update()
    {
        scoreText.text = scoreKeeper.GetScore().ToString("000000");
        healthFillImage.fillAmount = playerHeath.GetHealth() / maxHealthPlayer;
    }
}
