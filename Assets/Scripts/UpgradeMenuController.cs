using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Logika menu ulepszeń - toggle panelu, odświeżanie etykiet i interaktywności przycisków.
/// Cała struktura UI (przycisk, panel, wiersze) jest tworzona w GameScene i
/// przypisywana tu przez Inspectora. Pauzuje grę gdy panel jest otwarty.
/// </summary>
public class UpgradeMenuController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] GameObject     upgradePanel;
    [SerializeField] TextMeshProUGUI scoreText;

    [Header("Damage")]
    [SerializeField] Button          damageBuyBtn;
    [SerializeField] Image           damageBtnImg;
    [SerializeField] TextMeshProUGUI damageInfoText;

    [Header("Reload")]
    [SerializeField] Button          reloadBuyBtn;
    [SerializeField] Image           reloadBtnImg;
    [SerializeField] TextMeshProUGUI reloadInfoText;

    [Header("Health")]
    [SerializeField] Button          healthBuyBtn;
    [SerializeField] Image           healthBtnImg;
    [SerializeField] TextMeshProUGUI healthInfoText;

    public static bool IsOpen { get; private set; }

    static readonly Color colAffordable   = new Color(0.10f, 0.38f, 0.18f, 1f);
    static readonly Color colUnaffordable = new Color(0.38f, 0.10f, 0.10f, 1f);
    static readonly Color colMaxed        = new Color(0.22f, 0.22f, 0.22f, 1f);

    UpgradeManager upgrades;
    ScoreKeeper    scoreKeeper;

    void Start()
    {
        upgrades    = GetComponent<UpgradeManager>();
        scoreKeeper = FindFirstObjectByType<ScoreKeeper>();

        damageBuyBtn.onClick.AddListener(() => upgrades.TryBuy(UpgradeManager.UpgradeType.Damage));
        reloadBuyBtn.onClick.AddListener(() => upgrades.TryBuy(UpgradeManager.UpgradeType.Reload));
        healthBuyBtn.onClick.AddListener(() => upgrades.TryBuy(UpgradeManager.UpgradeType.Health));

        upgrades.OnUpgradeChanged += RefreshLabels;

        upgradePanel.SetActive(false);
        RefreshLabels();
    }

    void OnDestroy()
    {
        if (upgrades != null) upgrades.OnUpgradeChanged -= RefreshLabels;
        IsOpen         = false;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.uKey.wasPressedThisFrame)
            {
                TogglePanel();
            }
            else if (IsOpen && Keyboard.current.mKey.wasPressedThisFrame)
            {
                TogglePanel();
            }
        }
    }

    /// <summary>
    /// Otwiera/zamyka panel. Zamyka menu audio jeśli jest otwarte.
    /// </summary>
    public void TogglePanel()
    {
        bool show = !upgradePanel.activeSelf;

        // Jeśli otwieramy menu ulepszeń, upewnij się że menu audio jest zamknięte
        if (show && MusicController.IsOpen)
        {
            var musicMenu = FindFirstObjectByType<MusicController>();
            if (musicMenu != null) musicMenu.ToggleSettingsPanel();
        }

        upgradePanel.SetActive(show);
        IsOpen         = show;
        Time.timeScale = show ? 0f : 1f;

        if (show) RefreshLabels();
    }

    void RefreshLabels()
    {
        if (upgrades == null || scoreKeeper == null) return;

        scoreText.text = "PTS: " + scoreKeeper.GetScore();

        RefreshRow(UpgradeManager.UpgradeType.Damage, damageBuyBtn, damageBtnImg, damageInfoText);
        RefreshRow(UpgradeManager.UpgradeType.Reload, reloadBuyBtn, reloadBtnImg, reloadInfoText);
        RefreshRow(UpgradeManager.UpgradeType.Health, healthBuyBtn, healthBtnImg, healthInfoText);
    }

    void RefreshRow(UpgradeManager.UpgradeType type, Button btn, Image img, TextMeshProUGUI info)
    {
        int level = upgrades.GetLevel(type);

        if (level >= UpgradeManager.MaxLevel)
        {
            info.text        = $"Lv {level}/{UpgradeManager.MaxLevel}  MAX";
            btn.interactable = false;
            img.color        = colMaxed;
            return;
        }

        int  cost       = upgrades.GetCost(type);
        bool affordable = scoreKeeper.GetScore() >= cost;

        info.text        = $"Lv {level}/{UpgradeManager.MaxLevel}  {cost} pts";
        btn.interactable = affordable;
        img.color        = affordable ? colAffordable : colUnaffordable;
    }
}
