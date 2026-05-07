using System.Collections;
using UnityEngine;

/// <summary>
/// Zarządza zdrowiem gracza i wrogów — obsługuje obrażenia, tarczę, śmierć i spawning power-upów.
/// Rozróżnia zachowanie gracza (GameOver) od wrogów (dodanie wyniku, power-up przy śmierci).
/// </summary>
public class Health : MonoBehaviour
{
    [SerializeField] bool isPlayer;
    [SerializeField] int scoreValue = 50;
    [SerializeField] int health;
    [SerializeField] ParticleSystem hitParticles;
    [SerializeField] bool applyCameraShake;
    [SerializeField] ShieldAnimationManager shieldAnimationManager;

    int maxHealth;

    CameraShake cameraShake;
    AudioManager audioManager;
    ScoreKeeper scoreKeeper;
    LevelManager levelManager;
    bool isShieldActive = false;


    PowerUpSpawner powerUpSpawner;

    /// <summary>
    /// Inicjalizuje maksymalne zdrowie i znajduje referencje do systemów (CameraShake, AudioManager, itp.).
    /// </summary>
    void Start()
    {
        maxHealth = health;
        cameraShake = Camera.main.GetComponent<CameraShake>();
        audioManager = FindFirstObjectByType<AudioManager>();
        scoreKeeper = FindFirstObjectByType<ScoreKeeper>();
        levelManager = FindFirstObjectByType<LevelManager>();
        powerUpSpawner = FindFirstObjectByType<PowerUpSpawner>();
    }

    /// <summary>
    /// Zwraca aktualne zdrowie.
    /// </summary>
    public int GetHealth()
    {
        return health;
    }

    /// <summary>
    /// Zwiększa zdrowie o podaną ilość, ale nie przekracza maksymalnego zdrowia.
    /// </summary>
    public void Heal(int amount)
    {
        health = Mathf.Min(health + amount, maxHealth);
    }

    /// <summary>
    /// Obsługuje kolizję z projektylami — pobiera obrażenia, odtwarza efekty i dźwięk.
    /// </summary>
    void OnTriggerEnter2D(Collider2D collision)
    {
        DamageDealer damageDealer = collision.GetComponent<DamageDealer>();

        if (damageDealer != null)
        {
            TakeDamage(damageDealer.GetDamage());
            PlayHitParticles();
            damageDealer.Hit();
            audioManager.PlayTakeDamageSFX();

            if (applyCameraShake)
            {
                cameraShake.Play();
            }
        }
    }

    /// <summary>
    /// Aplikuje obrażenia do zdrowia — jeśli tarcza jest aktywna, absorbuje jedno trafienie i się dezaktywuje.
    /// </summary>
    void TakeDamage(int damage)
    {
        if (isShieldActive)
        {
            damage = 0;
            DeactivateShield();
        }

        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Obsługuje śmierć — dla gracza ładuje scenę GameOver, dla wrogów dodaje punkty i spawnia power-up.
    /// </summary>
    void Die()
    {
        if (isPlayer)
        {
            levelManager.LoadGameOver();
        }
        else
        {
            scoreKeeper.AddScore(scoreValue);
            powerUpSpawner.SpawnPowerUp(transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    /// <summary>
    /// Spawnia efekt cząstek przy trafieniu — niszczy efekt po zakończeniu animacji.
    /// </summary>
    void PlayHitParticles()
    {
        if (hitParticles != null)
        {
            ParticleSystem particles = Instantiate(hitParticles, transform.position, Quaternion.identity);
            Destroy(particles, particles.main.duration + particles.main.startLifetime.constantMax);
        }
    }

    /// <summary>
    /// Zwraca czy ten obiekt jest graczem czy wrogiem.
    /// </summary>
    public bool GetIsPlayer()
    {
        return isPlayer;
    }

    /// <summary>
    /// Aktywuje tarczę — następne trafienie będzie zaabsorbowane, następnie tarcza się dezaktywuje.
    /// </summary>
    public void ActivateShield()
    {
        isShieldActive = true;
        shieldAnimationManager.StartShieldAnimation();
    }

    /// <summary>
    /// Dezaktywuje tarczę — zatrzymuje animację i umożliwia zadanie obrażeń.
    /// </summary>
    public void DeactivateShield()
    {
        isShieldActive = false;
        shieldAnimationManager.StopShieldAnimation();
    }
}
