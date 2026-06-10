using UnityEngine;

/// <summary>
/// Steruje animacją postaci gracza podczas ładowania strzału.
/// Zmienia stan Animator na podstawie fazy ładowania.
/// </summary>
public class ChargingAnimationManager : MonoBehaviour
{
    Animator animator;

    /// <summary>
    /// Znajduje komponent Animator na obiekcie.
    /// </summary>
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Uruchamia animację ładowania - zmienia parametr "isCharging" na true.
    /// </summary>
    public void PlayChargingAnimation()
    {
        animator.SetBool("isCharging", true);
    }

    /// <summary>
    /// Zatrzymuje animację ładowania - zmienia parametr "isCharging" na false.
    /// </summary>
    public void StopChargingAnimation()
    {
        animator.SetBool("isCharging", false);
    }
}
