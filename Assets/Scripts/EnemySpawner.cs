using System.Collections;
using UnityEngine;

/// <summary>
/// Spawnia fale wrogów zgodnie z konfiguracją WaveConfigSO — skaluje pozycje spawnu do rozdzielczości ekranu.
/// Obsługuje pętle fal i opóźnienia między nimi.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] WaveConfigSO[] waveConfigs;
    [SerializeField] float timeBetweenWaves = 0f;
    [SerializeField] bool isLooping;
    [SerializeField] float designHalfWidth = 6f;
    WaveConfigSO currentWave;
    float pathScaleX;

    /// <summary>
    /// Oblicza skalę ścieżki na podstawie rozdzielczości ekranu i uruchamia coroutine spawning wrogów.
    /// </summary>
    void Start()
    {
        Camera cam = Camera.main;
        pathScaleX = (cam.orthographicSize * cam.aspect) / designHalfWidth;
        StartCoroutine(SpawnEnemies());
    }

    /// <summary>
    /// Coroutine: spawnia wrogów według fal z opóźnieniami — każdego wroga w pozycji startowej ze skalowaniem X.
    /// Pętla się jeśli isLooping jest true.
    /// </summary>
    IEnumerator SpawnEnemies()
    {
        do
        {
            foreach(WaveConfigSO wave in waveConfigs)
            {
                currentWave = wave;
                for (int i = 0; i < currentWave.GetEnemyCount(); i++)
                {
                    Vector3 spawnPos = currentWave.GetStartingWaypoint().position;
                    spawnPos.x *= pathScaleX;
                    Instantiate(
                        currentWave.GetEnemyPrefab(i),
                        spawnPos,
                        Quaternion.identity,
                        transform
                    );

                    yield return new WaitForSeconds(currentWave.GetRandomEnemySpawnTime());
                }
                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }
        while(isLooping);
    }

    /// <summary>
    /// Zwraca aktualnie spawnawaną falę.
    /// </summary>
    public WaveConfigSO GetCurrentWave() => currentWave;

    /// <summary>
    /// Zwraca skalę ścieżki dla rozdzielczości ekranu — używane do skalowania pozycji spawnu.
    /// </summary>
    public float GetPathScaleX() => pathScaleX;
}
