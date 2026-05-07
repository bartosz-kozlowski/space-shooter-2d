using System.Collections;
using UnityEngine;

/// <summary>
/// Efekt drżenia kamery — tworzy wrażenie wstrząsu при trafieniu.
/// Losowo przesuwa kamerę na określony czas, a następnie powraca do pierwotnej pozycji.
/// </summary>
public class CameraShake : MonoBehaviour
{
    [SerializeField] float shakeDuration = 0.5f;      // Czas trwania efektu drżenia
    [SerializeField] float shakeMagnitude = 0.5f;     // Siła drżenia (zakres ruchu losowego)

    Vector3 initialPosition;                          // Początkowa pozycja kamery

    /// <summary>
    /// Zapisuje początkową pozycję kamery.
    /// </summary>
    void Start()
    {
        initialPosition = transform.position;
    }

    /// <summary>
    /// Uruchamia efekt drżenia kamery.
    /// </summary>
    public void Play()
    {
        StartCoroutine(ShakeCamera());
    }

    /// <summary>
    /// Coroutine: losowo przesuwa kamerę przez określony czas, a następnie powraca do pozycji wyjściowej.
    /// </summary>
    IEnumerator ShakeCamera()
    {
        float timeElapsed = 0;

        while (timeElapsed < shakeDuration)
        {
            transform.position = initialPosition + (Vector3)Random.insideUnitCircle * shakeMagnitude;
            timeElapsed += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        transform.position = initialPosition;
    }
}
