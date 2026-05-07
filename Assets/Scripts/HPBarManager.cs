using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Zarządza wyglądem paska zdrowia — ustawia sprite paska zależnie od wybranego statku.
/// Każdy statek ma inny wygląd paska zdrowia w UI.
/// </summary>
public class HPBarManager : MonoBehaviour
{
    [SerializeField] Sprite[] healthBarSprites;  // Sprite'y pasków dla różnych statków (indeks: 0=Blue, 1=Orange)
    [SerializeField] Image fillImage;            // Komponenta UI wyświetlająca pasek

    CharSelectManager charSelectManager;

    /// <summary>
    /// Znajduje CharSelectManager na scenie.
    /// </summary>
    void Awake()
    {
        charSelectManager = FindFirstObjectByType<CharSelectManager>();
    }

    /// <summary>
    /// Ustawia sprite paska zdrowia na podstawie wybranego statku.
    /// </summary>
    void Start()
    {
        fillImage.sprite = healthBarSprites[charSelectManager.GetCurrentShipIndex()];
    }
}
