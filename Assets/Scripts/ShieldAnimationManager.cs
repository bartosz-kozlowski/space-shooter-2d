using UnityEngine;

/// <summary>
/// Zarządza wyświetlaniem i aktywacją tarczy ochronnej gracza.
/// Aktywuje/dezaktywuje obiekt tarczy - Animator startuje automatycznie przy wlaczeniu.
/// </summary>
public class ShieldAnimationManager : MonoBehaviour
{
    public void StartShieldAnimation()
    {
        gameObject.SetActive(true);
    }

    public void StopShieldAnimation()
    {
        gameObject.SetActive(false);
    }
}
