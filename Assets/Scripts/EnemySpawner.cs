using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] WaveConfigSO[] waveConfigs;
    [SerializeField] float timeBetweenWaves = 0f;
    [SerializeField] bool isLooping;
    [SerializeField] float designHalfWidth = 6f;
    WaveConfigSO currentWave;
    float pathScaleX;

    void Start()
    {
        Camera cam = Camera.main;
        pathScaleX = (cam.orthographicSize * cam.aspect) / designHalfWidth;
        StartCoroutine(SpawnEnemies());
    }

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

    public WaveConfigSO GetCurrentWave() => currentWave;
    public float GetPathScaleX() => pathScaleX;
}
