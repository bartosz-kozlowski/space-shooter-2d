using UnityEngine;

/// <summary>
/// Komponent fizyki power-upa — odpowiada za spadanie i aplikowanie efektu gdy gracz go zbierze.
/// Power-up pada w dół (przy użyciu Rigidbody2D) i aktywuje się przy kolizji z graczem.
/// </summary>
public class PowerUpManager : MonoBehaviour
{
    [SerializeField] PowerUpSO powerUp;          // Skryptowany obiekt określający efekt power-upa
    [SerializeField] float dropSpeed = 5f;       // Prędkość spadania power-upa

    Rigidbody2D powerUpRB;

    /// <summary>
    /// Znajduje komponent Rigidbody2D do zarządzania fizyką.
    /// </summary>
    void Awake()
    {
        powerUpRB = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Aktualizuje prędkość power-upa co klatkę — utrzymuje stały spadek.
    /// </summary>
    void Update()
    {
        AddVelocityToPowerUp();
    }

    /// <summary>
    /// Nadaje power-upowi prędkość spadania w dół ekranu.
    /// </summary>
    void AddVelocityToPowerUp()
    {
        powerUpRB.linearVelocity = -transform.up * dropSpeed;
    }

    /// <summary>
    /// Detekcja kolizji — jeśli gracz dotknie power-upa, zastosuj efekt i zniszcz obiekt.
    /// Weryfikuje kolizję zarówno poprzez layer jak i komponenty.
    /// </summary>
    void OnTriggerEnter2D(Collider2D collision)
    {
        int playerIndexLayer = LayerMask.NameToLayer("Player");
        // Weryfikacja czy to gracz — sprawdzenie layera i komponentów
        Shooter shooter = collision.GetComponentInChildren<Shooter>();
        Health health = collision.GetComponent<Health>();
        bool isPlayer = (shooter != null) || (health != null);

        if (collision.gameObject.layer == playerIndexLayer && isPlayer)
        {
            // Zastosuj efekt power-upa (wirtualnie wołane przez podklasy)
            powerUp.Apply(collision.gameObject);
            Destroy(gameObject);
        }
    }
}
