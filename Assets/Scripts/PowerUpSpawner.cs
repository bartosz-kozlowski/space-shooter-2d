using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zarządza spawnem power-upów przy śmierci wrogów.
/// Implementuje system prawdopodobieństwa drop - power-up pojawia się jeśli losowy drop rate > próg.
/// Ogranicza liczbę aktywnych power-upów na ekranie i aktywuje cooldown po osiągnięciu limitu.
/// </summary>
public class PowerUpSpawner : MonoBehaviour
{
    const float MAXDROPRATE = 1f;               // Maksymalne prawdopodobieństwo drop
    const float DROPRATETHRESHOLD = 0.5f;       // Próg drop - jeśli dropRate >= 0.5 pojawia się power-up
    float minDropRate = 0f;                     // Minimalne prawdopodobieństwo - zwiększa się przy brakach drop

    [SerializeField] List<GameObject> powerUps;             // Lista prefabów power-upów do losowania
    [SerializeField] int maximumPowerUpCount = 3;           // Maksymalna liczba power-upów na ekranie
    [SerializeField] float powerUpCoolDownDuration = 10f;   // Czas cooldownu po osiągnięciu limitu
    [SerializeField] float powerUpExistDuration = 8f;       // Jak długo power-up pozostaje na ekranie

    int powerUpCount = 0;                       // Bieżąca liczba aktywnych power-upów
    Coroutine coolDownCoroutine;                // Referencja do coroutine'a cooldownu

    /// <summary>
    /// Zwiększa licznik aktywnych power-upów o 1.
    /// </summary>
    public void IncreasePowerUpCount()
    {
        powerUpCount += 1;
    }

    /// <summary>
    /// Uruchamia cooldown - blokuje spawn power-upów na określony czas.
    /// </summary>
    public void StartCoolDownCoroutine()
    {
        if (coolDownCoroutine == null)
        {
            coolDownCoroutine = StartCoroutine(CoolDownCoroutine());
        }
    }

    /// <summary>
    /// Cooldown coroutine - resetuje licznik power-upów i czeka na koniec cooldownu.
    /// </summary>
    IEnumerator CoolDownCoroutine()
    {
        yield return new WaitForSeconds(powerUpCoolDownDuration);
        powerUpCount = 0;
        coolDownCoroutine = null;
    }

    /// <summary>
    /// Główna metoda - sprawdza czy można spawnować power-up, jeśli nie - aktywuje cooldown.
    /// </summary>
    public void SpawnPowerUp(Vector3 position, Quaternion rotation)
    {
        if (powerUpCount < maximumPowerUpCount)
        {
            CreatePowerUpDrop(position, rotation);
        }
        else
        {
            StartCoolDownCoroutine();
        }
    }

    /// <summary>
    /// Generuje losowe prawdopodobieństwo drop w podanym zakresie.
    /// </summary>
    float GenerateRandomRate(float minRange, float maxRange)
    {
        float rate = Random.Range(minRange, maxRange);
        return rate;
    }

    /// <summary>
    /// Wybiera losowy power-up z listy dostępnych.
    /// </summary>
    GameObject GetRandomPowerUp()
    {
        int powerUpIndex = Random.Range(0, powerUps.Count);
        return powerUps[powerUpIndex];
    }

    /// <summary>
    /// Próbuje spawnować power-up na podstawie losowego drop rate.
    /// Jeśli dropRate >= próg - spawnia power-up i zwiększa licznik.
    /// Jeśli nie - zwiększa minDropRate dla następnego podejścia.
    /// </summary>
    public void CreatePowerUpDrop(Vector3 position, Quaternion rotation)
    {
        if (minDropRate > 0.5f) minDropRate = 0f;

        float dropRate = GenerateRandomRate(minDropRate, MAXDROPRATE);

        Debug.Log($"[PowerUpSpawner] count={powerUpCount}/{maximumPowerUpCount} dropRate={dropRate:F2}");

        if (dropRate >= DROPRATETHRESHOLD)
        {
            GameObject powerUp = GetRandomPowerUp();
            Debug.Log($"[PowerUpSpawner] Spawning: {(powerUp != null ? powerUp.name : "NULL")}");
            GameObject instance = Instantiate(powerUp, position, rotation);
            Destroy(instance, powerUpExistDuration);

            powerUpCount += 1;
        }
        else
        {
            minDropRate += 0.1f;
        }
    }
}
