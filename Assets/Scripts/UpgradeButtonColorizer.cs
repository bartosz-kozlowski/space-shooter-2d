using TMPro;
using UnityEngine;

/// <summary>
/// Koloruje tekst przycisku UPGRADE na kolor wybranej postaci.
/// Komponent na obiekcie z komponentem TextMeshProUGUI (np. etykieta "UPGRADE" w lewym górnym rogu).
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class UpgradeButtonColorizer : MonoBehaviour
{
    [SerializeField] Color blueColor   = new Color(0x49 / 255f, 0xFF / 255f, 0xF3 / 255f, 1f);
    [SerializeField] Color orangeColor = new Color(1f, 0.5f, 0f, 1f);

    void Start()
    {
        TextMeshProUGUI label = GetComponent<TextMeshProUGUI>();
        CharSelectManager charSelect = FindFirstObjectByType<CharSelectManager>();
        if (charSelect == null) return;

        label.color = charSelect.GetCurrentShipIndex() == 0 ? blueColor : orangeColor;
    }
}
