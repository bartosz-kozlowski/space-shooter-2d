using UnityEngine;

/// <summary>
/// Zarządza wyświetlaniem i aktywacją tarczy ochronnej gracza.
/// Włącza/wyłącza sprite tarczy i jej collider.
/// </summary>
public class ShieldAnimationManager : MonoBehaviour
{
    SpriteRenderer shieldSprite;        // Grafika tarczy
    CircleCollider2D shieldCollider;    // Collider tarczy

    /// <summary>
    /// Znajduje komponenty sprite i collider tarczy.
    /// </summary>
    void Awake()
    {
        shieldSprite = GetComponent<SpriteRenderer>();
        shieldCollider = GetComponent<CircleCollider2D>();
    }

    /// <summary>
    /// Aktywuje tarczę — wyświetla sprite i włącza collider.
    /// </summary>
    public void StartShieldAnimation()
    {
        shieldSprite.enabled = true;
        shieldCollider.enabled = true;
    }

    /// <summary>
    /// Dezaktywuje tarczę — ukrywa sprite i wyłącza collider.
    /// </summary>
    public void StopShieldAnimation()
    {
        shieldSprite.enabled = false;
        shieldCollider.enabled = false;
    }
}
