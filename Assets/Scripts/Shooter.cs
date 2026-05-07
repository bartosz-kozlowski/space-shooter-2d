using System.Collections;
using UnityEngine;

/// <summary>
/// Skrypt odpowiedzialny za mechanikę strzelania gracza oraz przeciwników (AI).
/// Obsługuje strzały zwykłe, ładowane oraz ulepszenie MultiShot.
/// </summary>
public class Shooter : MonoBehaviour
{
    [Header("Podstawowe parametry pocisku")]
    [SerializeField] GameObject projectilePrefab;       // Prefab zwykłego pocisku
    [SerializeField] GameObject chargeProjectilePrefab; // Prefab naładowanego pocisku
    [SerializeField] float projectileSpeed = 10f;       // Prędkość zwykłego pocisku
    [SerializeField] float projectileLifetime = 5f;    // Czas życia pocisku przed zniszczeniem
    [SerializeField] float chargedProjectileSpeed = 10; // Prędkość naładowanego pocisku

    [Header("Parametry Sztucznej Inteligencji (AI)")]
    [SerializeField] bool useAI;                        // Czy obiekt ma strzelać automatycznie (dla wrogów)
    [SerializeField] float baseFireRate = 0.2f;         // Podstawowa częstotliwość strzału
    [SerializeField] float minimumFireRate = 0.2f;      // Minimalna możliwa przerwa między strzałami
    [SerializeField] float fireRateVariance = 0f;       // Losowa wariacja czasu strzału (dla naturalności)

    [HideInInspector] public bool isFiring = false;     // Czy trwa strzelanie zwykłe
    [HideInInspector] public bool isCharging = false;   // Czy trwa ładowanie strzału
    [HideInInspector] public bool chargeFiring = false; // Czy wystrzelono naładowany pocisk
    
    Coroutine fireCoroutine;        // Referencja do pętli ciągłego strzelania
    Coroutine multiShotCoroutine;   // Referencja do czasu trwania MultiShot
    AudioManager audioManager;      // Zarządzanie dźwiękiem
    int extraProjectile = 0;        // Liczba dodatkowych pocisków (z power-upa)

    /// <summary>
    /// Znajduje AudioManager na scenie do odtwarzania dźwięków.
    /// </summary>
    void Awake()
    {
        audioManager = FindFirstObjectByType<AudioManager>();
    }

    /// <summary>
    /// Inicjalizuje strzelanie — jeśli to AI (wróg), automatycznie zaczyna strzelać.
    /// </summary>
    void Start()
    {
        if (useAI)
        {
            isFiring = true;
        }
    }

    /// <summary>
    /// Aktualizuje stan strzelania co klatkę.
    /// </summary>
    void Update()
    {
        Fire();
    }

    /// <summary>
    /// Sprawdza stan isFiring i uruchamia lub zatrzymuje pętlę strzelania.
    /// </summary>
    void Fire()
    {
        if (isFiring && fireCoroutine == null)
        {
            fireCoroutine = StartCoroutine(FireContinously());
        }
        else if (!isFiring && fireCoroutine != null)
        {
            StopCoroutine(fireCoroutine);
            fireCoroutine = null;
        }
    }

    /// <summary>
    /// Oblicza losowy czas oczekiwania między strzałami (uwzględniając wariację).
    /// </summary>
    float CreateWaitTime()
    {
        float waitTime = Random.Range(baseFireRate - fireRateVariance, baseFireRate + fireRateVariance);
        waitTime = Mathf.Clamp(waitTime, minimumFireRate, float.MaxValue);
        return waitTime;
    }

    /// <summary>
    /// Tworzy instancję pocisku, nadaje mu prędkość i kierunek.
    /// </summary>
    GameObject CreateProjectiles(GameObject prefab, float speed, Transform transf, Quaternion rotation, bool isMultiShot)
    {
        GameObject projectile = Instantiate(prefab, transf.position, rotation);
        Rigidbody2D projectileRB = projectile.GetComponent<Rigidbody2D>();
        Vector3 upVector;

        // Ustal kierunek lotu: góra pocisku (dla MultiShot) lub góra lufy (zwykły strzał)
        if (isMultiShot)
        {
            upVector = projectile.transform.up;
        }
        else
        {
            upVector = transf.up;
        }

        projectileRB.linearVelocity = upVector * speed;
        return projectile;
    }

    /// <summary>
    /// Pętla strzelająca pociskami w regularnych odstępach czasu.
    /// </summary>
    IEnumerator FireContinously()
    {
        while (true)
        {
            // Stwórz główny pocisk
            GameObject projectile = CreateProjectiles(projectilePrefab, projectileSpeed, transform, transform.rotation, false);
            Destroy(projectile, projectileLifetime);
            
            // Sprawdź czy wystrzelić dodatkowe pociski z boku
            CheckMultiShot();

            float waitTime = CreateWaitTime();
            audioManager.PlayShootingSFX();

            yield return new WaitForSeconds(waitTime);
        }
    }

    /// <summary>
    /// Wystrzeliwuje pojedynczy pocisk (używane przy szybkich kliknięciach).
    /// </summary>
    public void FireOnce()
    {
        GameObject projectile = CreateProjectiles(projectilePrefab, projectileSpeed, transform, transform.rotation, false);
        Destroy(projectile, projectileLifetime);
        CheckMultiShot();
        audioManager.PlayShootingSFX();
    }

    /// <summary>
    /// Wystrzeliwuje potężny naładowany pocisk.
    /// </summary>
    public void ChargeFire()
    {
        GameObject chargeProjectile = CreateProjectiles(chargeProjectilePrefab, chargedProjectileSpeed, transform, transform.rotation, false);
        Destroy(chargeProjectile, projectileLifetime);
    }

    /// <summary>
    /// Obsługuje strzelanie wieloma pociskami naraz pod różnymi kątami.
    /// </summary>
    void CheckMultiShot()
    {
        if (!useAI && extraProjectile > 0)
        {
            float angle;
            Quaternion rotation;
            int totalProjectiles = extraProjectile + 1;

            for (int i = 0; i < totalProjectiles; i++)
            {
                // Oblicz kąt rozproszenia dla każdego dodatkowego pocisku
                angle = (i - (totalProjectiles - 1) / 2f) * 15f;

                if (angle == 0) continue; // Środkowy pocisk jest już stworzony w pętli głównej

                rotation = Quaternion.Euler(0, 0, angle);
                GameObject projectile = CreateProjectiles(projectilePrefab, projectileSpeed, transform, rotation, true);
                Destroy(projectile, projectileLifetime);
            }
        }
    }

    /// <summary>
    /// Uruchamia ulepszenie MultiShot na określony czas.
    /// </summary>
    public void CallMultiShotCoroutine(int amount, float duration)
    {
        if (multiShotCoroutine != null)
        {
            StopCoroutine(multiShotCoroutine);
            extraProjectile = 0;
        }

        multiShotCoroutine = StartCoroutine(MultiShot(amount, duration));
    }

    /// <summary>
    /// Współprogram zarządzający czasem trwania efektu MultiShot.
    /// </summary>
    public IEnumerator MultiShot(int amount, float duration)
    {
        extraProjectile += amount;
        yield return new WaitForSeconds(duration);
        extraProjectile -= amount;
    }

    /// <summary>
    /// Zwraca czy ten obiekt używa AI (automatyczne strzelanie).
    /// </summary>
    public bool GetUseAI()
    {
        return useAI;
    }
}
