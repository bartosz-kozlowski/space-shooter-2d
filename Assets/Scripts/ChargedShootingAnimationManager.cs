using UnityEngine;

/// <summary>
/// Steruje animacją naładowanego strzelania gracza.
/// Włącza/wyłącza animację na podstawie parametru Animator'a.
/// </summary>
public class ChargedShootingAnimationManager : MonoBehaviour
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
    /// Uruchamia animację naładowanego strzelania - zmienia parametr "isShooting" na true.
    /// </summary>
    public void PlayChargedShootingAnimation()
    {
        animator.SetBool("isShooting", true);
    }

    /// <summary>
    /// Zatrzymuje animację naładowanego strzelania - zmienia parametr "isShooting" na false.
    /// </summary>
    public void StopChargedShootingAnimation()
    {
        animator.SetBool("isShooting", false);
    }
}
