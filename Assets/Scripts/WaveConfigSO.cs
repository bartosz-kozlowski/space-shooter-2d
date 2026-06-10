using UnityEngine;

/// <summary>
/// Konfiguracja fali przeciwników przechowywana w pliku ScriptableObject.
/// Zawiera prefaby wrogów, trasę poruszania się oraz parametry spawnu.
/// </summary>
[CreateAssetMenu(fileName = "WaveConfig", menuName = "New WaveConfig")]
public class WaveConfigSO : ScriptableObject
{
    [SerializeField] GameObject[] enemyPrefabs;      // Prefaby dostępnych typów wrogów
    [SerializeField] Transform pathPrefab;           // Transform zawierający waypoints trasy
    [SerializeField] float enemyMoveSpeed = 5f;      // Prędkość poruszania się wroga
    [SerializeField] float timeBetweenEnemySpawn = 1f; // Przerwa między spawnami
    [SerializeField] float enemySpawnVariance = 0f;  // Losowa wariacja czasu spawnu
    [SerializeField] float minimumSpawnTime = 0.2f;  // Minimalna przerwa między spawnami

    /// <summary>
    /// Zwraca pierwszy waypoint trasy (punkt startu dla wroga).
    /// </summary>
    public Transform GetStartingWaypoint()
    {
        return pathPrefab.GetChild(0);
    }

    /// <summary>
    /// Zwraca prędkość poruszania się wroga.
    /// </summary>
    public float GetEnemyMoveSpeed()
    {
        return enemyMoveSpeed;
    }

    /// <summary>
    /// Zwraca tablicę wszystkich waypointów dla trasy wroga.
    /// </summary>
    public Transform[] GetWaypoints()
    {
        Transform[] waypoints = new Transform[pathPrefab.childCount];

        for (int i = 0; i < pathPrefab.childCount; i++)
        {
            waypoints[i] = pathPrefab.GetChild(i);
        }

        return waypoints;
    }

    /// <summary>
    /// Zwraca liczbę wrogów do spawnu w tej fali.
    /// </summary>
    public int GetEnemyCount()
    {
        return enemyPrefabs.Length;
    }

    /// <summary>
    /// Zwraca prefab wroga na podstawie indeksu.
    /// </summary>
    public GameObject GetEnemyPrefab(int index)
    {
        return enemyPrefabs[index];
    }

    /// <summary>
    /// Oblicza losowy czas oczekiwania między spawnami (z wariacja).
    /// </summary>
    public float GetRandomEnemySpawnTime()
    {
        float spawnTime = Random.Range(timeBetweenEnemySpawn - enemySpawnVariance,
            timeBetweenEnemySpawn + enemySpawnVariance);

        spawnTime = Mathf.Clamp(spawnTime, minimumSpawnTime, float.MaxValue);
        return spawnTime;
    }
}