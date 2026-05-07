using UnityEngine;

/// <summary>
/// Zarządza efektami dźwiękowymi gry — strzały, obrażenia, ładowanie broni.
/// Odtwarza dźwięki za pomocą AudioSource.PlayClipAtPoint w pozycji kamery.
/// </summary>
public class AudioManager : MonoBehaviour
{
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

    /// <summary>
    /// Odtwarza dźwięk w pozycji kamery z podaną głośnością — jeśli klip jest dostępny.
    /// </summary>
    public void PlayAudioClip(AudioClip clip, float volume)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, volume);
        }
    }

    /// <summary>
    /// Odtwarza dźwięk strzału.
    /// </summary>
    public void PlayShootingSFX()
    {
        PlayAudioClip(shootingClip, shootingVolume);
    }

    /// <summary>
    /// Odtwarza dźwięk otrzymania obrażeń.
    /// </summary>
    public void PlayTakeDamageSFX()
    {
        PlayAudioClip(takeDamageClip, takeDamageVolumne);
    }

    /// <summary>
    /// Odtwarza dźwięk rozpoczęcia ładowania broni.
    /// </summary>
    public void PlayChargeUpSFX()
    {
        PlayAudioClip(chargeUpClip, chargeUpVolumne);
    }

    /// <summary>
    /// Odtwarza dźwięk wystrzelenia naładowanego strzału.
    /// </summary>
    public void PlayChargingShotSFX()
    {
        PlayAudioClip(chargingShotClip, chargingShotVolumne);
    }
    
}
