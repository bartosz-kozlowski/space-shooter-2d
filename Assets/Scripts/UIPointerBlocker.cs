using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Dodaj ten komponent do dowolnego elementu UI który ma blokować strzelanie
/// gdy kursor jest nad nim (np. przycisk UPGRADE).
/// </summary>
public class UIPointerBlocker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static bool IsBlocking { get; private set; }

    public void OnPointerEnter(PointerEventData eventData) => IsBlocking = true;
    public void OnPointerExit(PointerEventData eventData)  => IsBlocking = false;

    void OnDisable() => IsBlocking = false;
}
