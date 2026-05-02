using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MusicController : MonoBehaviour
{
    const string VOLUME_KEY = "MusicVolume";

    static readonly Color orangeFill = new Color(1f,           0.6f,          0.1f, 1f);
    static readonly Color blueFill   = new Color(0x49 / 255f,  0xFF / 255f,   0xF3 / 255f, 1f);

    [SerializeField] Sprite soundOnSprite;
    [SerializeField] Sprite soundOffSprite;

    Image      iconImage;
    Slider     volumeSlider;
    GameObject sliderPanel;
    float      lastNonZeroVolume = 1f;
    AudioSource musicSource;

    void Start()
    {
        musicSource = GetComponent<AudioSource>();
        float saved = PlayerPrefs.GetFloat(VOLUME_KEY, 0.5f);
        AudioListener.volume = saved;
        lastNonZeroVolume    = saved > 0f ? saved : 0.5f;

        Color fill = GetFillColor();
        BuildButton();
        BuildSlider(fill);

        sliderPanel.SetActive(false);

        if (volumeSlider != null)
            volumeSlider.SetValueWithoutNotify(saved);

        RefreshIcon(saved);
    }

    Color GetFillColor()
    {
        var cs = FindFirstObjectByType<CharSelectManager>();
        return (cs != null && cs.GetCurrentShipIndex() == 0) ? blueFill : orangeFill;
    }

    // Icon button — top-right corner, toggles slider panel
    void BuildButton()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        var root = new GameObject("MuteButton");
        root.transform.SetParent(canvas.transform, false);

        var rect = root.AddComponent<RectTransform>();
        rect.anchorMin        = new Vector2(1f, 1f);
        rect.anchorMax        = new Vector2(1f, 1f);
        rect.pivot            = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-25f, -25f);
        rect.sizeDelta        = new Vector2(80f, 80f);

        iconImage = root.AddComponent<Image>();
        iconImage.preserveAspect = true;

        var btn = root.AddComponent<Button>();
        btn.targetGraphic = iconImage;
        btn.onClick.AddListener(ToggleSliderPanel);
    }

    // Slider panel — hidden by default, appears to the left of the icon
    void BuildSlider(Color fillColor)
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        sliderPanel = new GameObject("VolumeSlider");
        sliderPanel.transform.SetParent(canvas.transform, false);

        var rootRect = sliderPanel.AddComponent<RectTransform>();
        rootRect.anchorMin        = new Vector2(1f, 1f);
        rootRect.anchorMax        = new Vector2(1f, 1f);
        rootRect.pivot            = new Vector2(1f, 1f);
        rootRect.anchoredPosition = new Vector2(-115f, -50f);
        rootRect.sizeDelta        = new Vector2(150f, 30f);

        var bg = sliderPanel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.5f);

        volumeSlider              = sliderPanel.AddComponent<Slider>();
        volumeSlider.minValue     = 0f;
        volumeSlider.maxValue     = 1f;
        volumeSlider.wholeNumbers = false;
        volumeSlider.direction    = Slider.Direction.LeftToRight;

        var fillAreaGo   = new GameObject("Fill Area");
        fillAreaGo.transform.SetParent(sliderPanel.transform, false);
        var fillAreaRect = fillAreaGo.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(5f,   3f);
        fillAreaRect.offsetMax = new Vector2(-15f, -3f);

        var fillGo   = new GameObject("Fill");
        fillGo.transform.SetParent(fillAreaGo.transform, false);
        var fillRect = fillGo.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0f, 1f);
        fillRect.sizeDelta = Vector2.zero;
        var fillImg  = fillGo.AddComponent<Image>();
        fillImg.color = fillColor;

        var handleAreaGo   = new GameObject("Handle Slide Area");
        handleAreaGo.transform.SetParent(sliderPanel.transform, false);
        var handleAreaRect = handleAreaGo.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(10f,  0f);
        handleAreaRect.offsetMax = new Vector2(-10f, 0f);

        var handleGo   = new GameObject("Handle");
        handleGo.transform.SetParent(handleAreaGo.transform, false);
        var handleRect = handleGo.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20f, 0f);
        handleRect.anchorMin = new Vector2(0f, 0f);
        handleRect.anchorMax = new Vector2(0f, 1f);
        var handleImg  = handleGo.AddComponent<Image>();
        handleImg.color = Color.white;

        volumeSlider.fillRect      = fillRect;
        volumeSlider.handleRect    = handleRect;
        volumeSlider.targetGraphic = handleImg;
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
            ToggleMute();
    }

    public static bool IsOpen { get; private set; }

    void ToggleSliderPanel()
    {
        if (sliderPanel == null) return;
        bool show = !sliderPanel.activeSelf;
        sliderPanel.SetActive(show);
        IsOpen         = show;
        Time.timeScale = show ? 0f : 1f;
    }

    void OnDestroy()
    {
        IsOpen         = false;
        Time.timeScale = 1f;
    }

    // M key: toggle between 0 and last known volume
    void ToggleMute()
    {
        if (AudioListener.volume > 0f)
        {
            lastNonZeroVolume = AudioListener.volume;
            ApplyVolume(0f);
        }
        else
        {
            ApplyVolume(lastNonZeroVolume);
        }
    }

    void OnVolumeChanged(float value)
    {
        if (value > 0f) lastNonZeroVolume = value;
        ApplyVolume(value);
    }

    void ApplyVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(VOLUME_KEY, value);
        PlayerPrefs.Save();
        float sourceVol = musicSource != null ? musicSource.volume : 1f;
        Debug.Log($"[MusicController] listener={value:F2} source={sourceVol:F2} final={value * sourceVol:F2}");

        if (volumeSlider != null)
            volumeSlider.SetValueWithoutNotify(value);

        RefreshIcon(value);
    }

    void RefreshIcon(float volume)
    {
        if (iconImage == null) return;
        bool silent = volume <= 0f;
        iconImage.sprite = silent ? soundOffSprite : soundOnSprite;
        iconImage.color  = silent ? new Color(1f, 1f, 1f, 0.45f) : Color.white;
    }
}
