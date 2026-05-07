using UnityEngine;

/// <summary>
/// Steruje animacją normalnego strzelania gracza.
/// Włącza/wyłącza animację na podstawie parametru Animator'a.
/// </summary>
public class NormalShootingAnimationManager : MonoBehaviour
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
    /// Uruchamia animację normalnego strzelania — zmienia parametr "isNormalShooting" na true.
    /// </summary>
    public void PlayNormalShootingAnimation()
    {
        animator.SetBool("isNormalShooting", true);
    }

    /// <summary>
    /// Zatrzymuje animację normalnego strzelania — zmienia parametr "isNormalShooting" na false.
    /// </summary>
    public void StopNormalShootingAnimation()
    {
        animator.SetBool("isNormalShooting", false);
    }
}
