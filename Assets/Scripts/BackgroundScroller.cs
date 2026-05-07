using UnityEngine;

/// <summary>
/// Efekt ruchu tła — przewija teksturę tła w kierunku określonym przez moveSpeed.
/// Tworzy wrażenie poruszającego się otoczenia bez przemieszczania obiektu.
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    [SerializeField] Vector2 moveSpeed;   // Prędkość przewijania tekstury (X, Y)

    Vector2 offset;                       // Bieżące przesunięcie tekstury
    Material material;                    // Material tła do modyfikacji

    /// <summary>
    /// Pobiera material sprite'a tła.
    /// </summary>
    void Start()
    {
        material = GetComponent<SpriteRenderer>().material;
    }

    /// <summary>
    /// Aktualizuje przesunięcie tekstury co klatkę — tworzy efekt przewijania.
    /// </summary>
    void Update()
    {
        offset += moveSpeed * Time.deltaTime;
        material.mainTextureOffset = offset;
    }
}
