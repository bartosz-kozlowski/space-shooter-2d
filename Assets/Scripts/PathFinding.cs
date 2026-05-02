using System;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    EnemySpawner enemySpawner;
    WaveConfigSO waveConfigSO;
    Vector3[] waypointPositions;
    int waypointIndex = 0;

    void Start()
    {
        enemySpawner = FindFirstObjectByType<EnemySpawner>();
        waveConfigSO = enemySpawner.GetCurrentWave();

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

    void Update()
    {
        FollowPath();
    }

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
