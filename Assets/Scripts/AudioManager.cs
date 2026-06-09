using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Zarządza efektami dźwiękowymi i muzyką w grze.
/// Implementuje wzorzec Singleton dla łatwego dostępu.
/// Integruje się z AudioMixerem dla profesjonalnej kontroli głośności.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer Settings")]
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] AudioMixerGroup sfxGroup;
    [SerializeField] string masterVolumeParam = "MasterVolume";
    [SerializeField] string musicVolumeParam  = "MusicVolume";
    [SerializeField] string sfxVolumeParam    = "SFXVolume";

    [Header("Shooting SFX")]
    [SerializeField] AudioClip shootingClip;
    [SerializeField] [Range(0, 1)] float shootingVolume = 1f;

    [Header("Take damage SFX")]
    [SerializeField] AudioClip takeDamageClip;
    [SerializeField] [Range(0, 1)] float takeDamageVolumne = 1f;

    [Header("Charge UP SFX")]
    [SerializeField] AudioClip chargeUpClip;
    [SerializeField] [Range(0, 1)] float chargeUpVolumne = 1f;

    [Header("Charging Shot SFX")]
    [SerializeField] AudioClip chargingShotClip;
    [SerializeField] [Range(0, 1)] float chargingShotVolumne = 1f;

    private AudioSource sfxSource;
    private AudioSource chargeLoopSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;

            chargeLoopSource = gameObject.AddComponent<AudioSource>();
            chargeLoopSource.playOnAwake = false;
            chargeLoopSource.loop = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (sfxSource != null)        sfxSource.outputAudioMixerGroup        = sfxGroup;
        if (chargeLoopSource != null) chargeLoopSource.outputAudioMixerGroup = sfxGroup;

        LoadVolumeSettings();
    }

    public void SetVolume(string parameterName, float sliderValue)
    {
        float dbValue = sliderValue > 0.0001f ? Mathf.Log10(sliderValue) * 20 : -80f;
        audioMixer.SetFloat(parameterName, dbValue);
        PlayerPrefs.SetFloat(parameterName, sliderValue);
    }

    private void LoadVolumeSettings()
    {
        SetVolume(masterVolumeParam, PlayerPrefs.GetFloat(masterVolumeParam, 0.75f));
        SetVolume(musicVolumeParam,  PlayerPrefs.GetFloat(musicVolumeParam,  0.75f));
        SetVolume(sfxVolumeParam,    PlayerPrefs.GetFloat(sfxVolumeParam,    0.75f));
    }

    /// <summary>
    /// Odtwarza dźwięk przez źródło przypisane do odpowiedniej grupy miksera.
    /// </summary>
    public void PlayAudioClip(AudioClip clip, float volume)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    public void PlayShootingSFX()     => PlayAudioClip(shootingClip, shootingVolume);
    public void PlayTakeDamageSFX()   => PlayAudioClip(takeDamageClip, takeDamageVolumne);
    public void PlayChargingShotSFX() => PlayAudioClip(chargingShotClip, chargingShotVolumne);

    public void PlayChargeUpSFX()      => PlayAudioClip(chargeUpClip, chargeUpVolumne);
    public void StopChargeUpSFX()     { }
}
