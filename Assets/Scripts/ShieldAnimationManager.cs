using UnityEngine;

/// <summary>
/// Zarządza wyświetlaniem i aktywacją tarczy ochronnej gracza.
/// Włącza/wyłącza sprite tarczy i jej collider.
/// </summary>
public class ShieldAnimationManager : MonoBehaviour
{
    SpriteRenderer shieldSprite;
    CircleCollider2D shieldCollider;

    void Awake()
    {
        shieldSprite = GetComponent<SpriteRenderer>();
        shieldCollider = GetComponent<CircleCollider2D>();
    }

    public void StartShieldAnimation()
    {
        shieldSprite.enabled = true;
        shieldCollider.enabled = true;
    }

    public void StopShieldAnimation()
    {
        shieldSprite.enabled = false;
        shieldCollider.enabled = false;
    }
}
