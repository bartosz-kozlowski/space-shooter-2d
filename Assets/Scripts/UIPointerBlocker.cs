using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Blokuje strzelanie gdy kursor znajduje sie nad elementem UI (np. przycisk UPGRADE).
/// </summary>
public class UIPointerBlocker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static bool IsBlocking { get; private set; }

    public void OnPointerEnter(PointerEventData eventData) => IsBlocking = true;
    public void OnPointerExit(PointerEventData eventData)  => IsBlocking = false;

    void OnDisable() => IsBlocking = false;
}
