using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Główny kontroler gracza — obsługuje ruch, strzelanie i mechanikę ładowania strzału.
/// Korzysta z Unity Input System do odczytywania wejścia i State Pattern do zarządzania stanami ładowania.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 0.3f;
    [SerializeField] float leftBoundPadding = 0f;
    [SerializeField] float rightBoundPadding = 0f;
    [SerializeField] float bottomBoundPadding = 0f;
    [SerializeField] float topBoundPadding = 0f;

    InputAction moveAction;
    Vector3 moveVector;
    Vector2 minBound;
    Vector2 maxBound;

    [SerializeField] Shooter playerShooter;
    InputAction fireAction;
    ChargingAnimationManager chargingAnimationManager;
    AudioManager audioManager;
    ChargeBarManager chargeBarManager;
    bool isPendingHold;                 // Stan: czeka się na próg 0.4s zanim przejdzie w ładowanie
    Coroutine chargeBarCoroutine;

    /// <summary>
    /// Inicjalizuje Input System akcje i znajduje menedżery systemów gry.
    /// </summary>
    void Awake()
    {
        fireAction = InputSystem.actions.FindAction("Fire");
        moveAction = InputSystem.actions.FindAction("Move");
        chargingAnimationManager = FindFirstObjectByType<ChargingAnimationManager>();
        audioManager = FindFirstObjectByType<AudioManager>();
        chargeBarManager = FindFirstObjectByType<ChargeBarManager>();
    }

    /// <summary>
    /// Oblicza granice ekranu na podstawie kamery — umożliwia ruch gracza tylko w obrębie ekranu.
    /// </summary>
    void Start()
    {
        InitializeBound();
    }

    /// <summary>
    /// Konwertuje koordinaty viewportu kamery na współrzędne świata.
    /// </summary>
    void InitializeBound()
    {
        Camera mainCamera = Camera.main;
        minBound = mainCamera.ViewportToWorldPoint(new Vector2(0, 0));
        maxBound = mainCamera.ViewportToWorldPoint(new Vector2(1, 1));
    }

    /// <summary>
    /// Aktualizuje ruch i strzelanie gracza co klatkę.
    /// </summary>
    void Update()
    {
        MovePlayer();
        FireShooter();
    }

    /// <summary>
    /// Porusza graczem na podstawie wejścia — ogranicza pozycję do granic ekranu.
    /// </summary>
    void MovePlayer()
    {
        moveVector = moveAction.ReadValue<Vector2>();
        Vector3 newPos = transform.position + (moveVector * moveSpeed * Time.deltaTime);

        newPos.x = Math.Clamp(newPos.x, minBound.x + leftBoundPadding, maxBound.x - rightBoundPadding);
        newPos.y = Math.Clamp(newPos.y, minBound.y + bottomBoundPadding, maxBound.y - topBoundPadding);

        transform.position = newPos;
    }

    /// <summary>
    /// Obsługuje logikę strzelania — maszyna stanów ładowania/normalnego ognia.
    /// Sprawdza czy menu nie jest otwarte (pauzuje strzelanie).
    /// </summary>
    void FireShooter()
    {
        if (MusicController.IsOpen)
        {
            playerShooter.isFiring = false;
            return;
        }

        if (playerShooter.isCharging)
        {
            playerShooter.isFiring = false;
            audioManager.PlayChargeUpSFX();

            if (playerShooter.chargeFiring)
            {
                audioManager.PlayChargingShotSFX();
                playerShooter.ChargeFire();
                playerShooter.chargeFiring = false;
                playerShooter.isCharging = false;
                chargingAnimationManager.StopChargingAnimation();
                chargeBarManager.StopChargeBarAnimation();
            }
        }
        else if (isPendingHold)
        {
            playerShooter.isFiring = false;
        }
        else
        {
            playerShooter.isFiring = fireAction.IsPressed();
        }
    }

    /// <summary>
    /// Rejestruje callbacki Input System dla przycisków Fire.
    /// </summary>
    void OnEnable()
    {
        fireAction.Enable();

        fireAction.started += OnHoldStarted;
        fireAction.performed += OnHoldPerformed;
        fireAction.canceled += OnHoldCanceled;
    }

    /// <summary>
    /// Wyrejestruje callbacki Input System.
    /// </summary>
    void OnDisable()
    {
        fireAction.started -= OnHoldStarted;
        fireAction.performed -= OnHoldPerformed;
        fireAction.canceled -= OnHoldCanceled;

        fireAction.Disable();
    }

    /// <summary>
    /// Callback: przycisk wciśnięty — wejście w stan isPendingHold.
    /// Opóźnienie 0.2s zanim pojawi się pasek ładowania (filtruje szybkie kliknięcia).
    /// </summary>
    void OnHoldStarted(InputAction.CallbackContext ctx)
    {
        if (MusicController.IsOpen) return;
        isPendingHold = true;
        chargeBarCoroutine = StartCoroutine(ShowChargeBarDelayed(0.2f));
    }

    /// <summary>
    /// Coroutine: opóźnienie przed pokazaniem paska ładowania.
    /// </summary>
    IEnumerator ShowChargeBarDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        chargeBarManager.PlayChargeBarAnimation();
    }

    /// <summary>
    /// Callback: przycisk przytrzymany dłużej — przejście do stanu ładowania.
    /// Uruchamia animację ładowania i zmienia stan isCharging na true.
    /// </summary>
    void OnHoldPerformed(InputAction.CallbackContext ctx)
    {
        if (MusicController.IsOpen) return;
        isPendingHold = false;
        playerShooter.isCharging = true;
        chargingAnimationManager.PlayChargingAnimation();
    }

    /// <summary>
    /// Callback: przycisk zwolniony — wyjście ze stanu ładowania.
    /// Jeśli gracz był w fazie ładowania → wystrzelenie naładowanego pocisku.
    /// Jeśli tylko szybkie kliknięcie → normalny pojedynczy strzał.
    /// </summary>
    void OnHoldCanceled(InputAction.CallbackContext ctx)
    {
        if (chargeBarCoroutine != null)
        {
            StopCoroutine(chargeBarCoroutine);
            chargeBarCoroutine = null;
        }
        isPendingHold = false;
        chargeBarManager.StopChargeBarAnimation();

        if (MusicController.IsOpen)
        {
            playerShooter.isCharging   = false;
            playerShooter.chargeFiring = false;
            chargingAnimationManager.StopChargingAnimation();
            return;
        }

        if (playerShooter.isCharging)
        {
            playerShooter.chargeFiring = true;
        }
        else
        {
            playerShooter.FireOnce();
        }
    }
}
