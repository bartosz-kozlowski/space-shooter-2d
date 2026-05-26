using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Prosty kontroler interfejsu audio.
/// Odpowiada za przekazywanie wartości z suwaków UI do AudioManager.
/// NIE buduje już interfejsu w kodzie — wymaga przypisania elementów w Inspektorze.
/// </summary>
public class MusicController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] GameObject settingsPanel;
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;
    
    [Header("Settings")]
    [SerializeField] string masterParam = "MasterVolume";
    [SerializeField] string musicParam  = "MusicVolume";
    [SerializeField] string sfxParam    = "SFXVolume";

    public static bool IsOpen { get; private set; }

    void Start()
    {
        // Inicjalizacja suwaków zapisanymi wartościami
        if (masterSlider) masterSlider.value = PlayerPrefs.GetFloat(masterParam, 0.75f);
        if (musicSlider)  musicSlider.value  = PlayerPrefs.GetFloat(musicParam,  0.75f);
        if (sfxSlider)    sfxSlider.value    = PlayerPrefs.GetFloat(sfxParam,    0.75f);

        // Dodanie listenerów - korzystamy z anonimowej funkcji, żeby zawsze odwoływać się do AKTUALNEJ instancji
        if (masterSlider) masterSlider.onValueChanged.AddListener(val => {
            if (AudioManager.Instance != null) AudioManager.Instance.SetVolume(masterParam, val);
        });
        
        if (musicSlider)  musicSlider.onValueChanged.AddListener(val => {
            if (AudioManager.Instance != null) AudioManager.Instance.SetVolume(musicParam, val);
        });
        
        if (sfxSlider)    sfxSlider.onValueChanged.AddListener(val => {
            if (AudioManager.Instance != null) AudioManager.Instance.SetVolume(sfxParam, val);
        });

        if (settingsPanel) settingsPanel.SetActive(false);
    }

    void Update()
    {
        // Jeśli panel jest otwarty i gracz naciśnie 'U' lub 'M', zamknij go
        // Jeśli jest zamknięty i gracz naciśnie 'M', otwórz go
        if (Keyboard.current != null)
        {
            if (Keyboard.current.mKey.wasPressedThisFrame)
            {
                ToggleSettingsPanel();
            }
            else if (IsOpen && Keyboard.current.uKey.wasPressedThisFrame)
            {
                ToggleSettingsPanel();
            }
        }
    }

    /// <summary>
    /// Przełącza widoczność panelu ustawień i pauzuje grę.
    /// Jeśli otwiera ten panel, zamyka panel ulepszeń.
    /// </summary>
    public void ToggleSettingsPanel()
    {
        if (settingsPanel == null) return;

        bool show = !settingsPanel.activeSelf;

        // Jeśli otwieramy menu audio, upewnij się że menu ulepszeń jest zamknięte
        if (show && UpgradeMenuController.IsOpen)
        {
            var upgradeMenu = FindFirstObjectByType<UpgradeMenuController>();
            if (upgradeMenu != null) upgradeMenu.TogglePanel();
        }

        settingsPanel.SetActive(show);
        IsOpen = show;

        // Pauza gry gdy menu jest otwarte
        Time.timeScale = show ? 0f : 1f;
    }

    void OnDestroy()
    {
        IsOpen = false;
        Time.timeScale = 1f;
    }
}
