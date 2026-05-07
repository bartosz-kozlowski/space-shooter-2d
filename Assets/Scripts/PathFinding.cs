using UnityEngine;

/// <summary>
/// Steruje poruszaniem się wroga wzdłuż predefiniowanej trasy (waypointów).
/// Wróg porusza się z prędkością określoną w WaveConfigSO.
/// Po dotarciu do ostatniego punktu trasy obiekt jest niszczony.
/// </summary>
public class PathFinding : MonoBehaviour
{
    EnemySpawner enemySpawner;
    WaveConfigSO waveConfigSO;
    Vector3[] waypointPositions; // Przeskalowane pozycje waypointów do rozdzielczości ekranu
    int waypointIndex = 0;       // Indeks bieżącego waypointa

    /// <summary>
    /// Inicjalizuje ścieżkę wroga — pobiera waypoints z bieżącej fali i przeskalowuje je do rozdzielczości.
    /// </summary>
    void Start()
    {
        enemySpawner = FindFirstObjectByType<EnemySpawner>();
        waveConfigSO = enemySpawner.GetCurrentWave();

        // Pobierz waypoints z konfiguracji fali i przeskaluj je
        float scaleX = enemySpawner.GetPathScaleX();
        Transform[] rawWaypoints = waveConfigSO.GetWaypoints();
        waypointPositions = new Vector3[rawWaypoints.Length];
        for (int i = 0; i < rawWaypoints.Length; i++)
        {
            Vector3 pos = rawWaypoints[i].position;
            pos.x *= scaleX;
            waypointPositions[i] = pos;
        }

        transform.position = waypointPositions[0];
    }

    /// <summary>
    /// Aktualizuje ruch wroga po trasie co klatkę.
    /// </summary>
    void Update()
    {
        FollowPath();
    }

    /// <summary>
    /// Porusza wrogiem do następnego waypointa. Po osiągnięciu ostatniego zniszcza obiekt.
    /// </summary>
    void FollowPath()
    {
        if (waypointIndex < waypointPositions.Length)
        {
            Vector3 targetPos = waypointPositions[waypointIndex];
            float moveDelta = waveConfigSO.GetEnemyMoveSpeed() * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, targetPos, moveDelta);

            if (transform.position == targetPos)
            {
                waypointIndex++;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
