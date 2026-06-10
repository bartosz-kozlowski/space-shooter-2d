using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Główny kontroler gracza - obsługuje ruch, strzelanie i mechanikę ładowania strzału.
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
    AudioManager _audioManager;
    AudioManager audioManager => _audioManager ??= AudioManager.Instance ?? FindFirstObjectByType<AudioManager>();
    ChargeBarManager chargeBarManager;
    bool isPendingHold;       // Przycisk wciśnięty, czeka na pełne naładowanie paska
    Coroutine chargeBarCoroutine;

    /// <summary>
    /// Inicjalizuje Input System akcje i znajduje menedżery systemów gry.
    /// </summary>
    void Awake()
    {
        fireAction = InputSystem.actions.FindAction("Fire");
        moveAction = InputSystem.actions.FindAction("Move");
        chargingAnimationManager = FindFirstObjectByType<ChargingAnimationManager>();

        chargeBarManager = FindFirstObjectByType<ChargeBarManager>();
        chargeBarManager.OnChargeFull += OnChargeBarFull;
    }

    void OnDestroy()
    {
        if (chargeBarManager != null)
            chargeBarManager.OnChargeFull -= OnChargeBarFull;
    }

    /// <summary>
    /// Oblicza granice ekranu na podstawie kamery - umożliwia ruch gracza tylko w obrębie ekranu.
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
    /// Porusza graczem na podstawie wejścia - ogranicza pozycję do granic ekranu.
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
    /// Zwraca true, gdy któreś z menu pauzujących grę jest otwarte
    /// (głośność lub ulepszenia) - wstrzymuje strzelanie.
    /// </summary>
    bool IsAnyMenuOpen()
    {
        return MusicController.IsOpen || UpgradeMenuController.IsOpen
               || UIPointerBlocker.IsBlocking;
    }

    /// <summary>
    /// Obsługuje logikę strzelania - maszyna stanów ładowania/normalnego ognia.
    /// Sprawdza czy menu nie jest otwarte (pauzuje strzelanie).
    /// </summary>
    void FireShooter()
    {
        if (IsAnyMenuOpen())
        {
            playerShooter.isFiring = false;
            return;
        }

        if (playerShooter.isCharging)
        {
            playerShooter.isFiring = false;

            if (playerShooter.chargeFiring)
            {
                audioManager.StopChargeUpSFX();
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
    /// Callback: przycisk wciśnięty - startuje pasek ładowania po 0.2s.
    /// Stan isPendingHold blokuje normalny ogień do czasu pełnego naładowania lub puszczenia.
    /// </summary>
    void OnHoldStarted(InputAction.CallbackContext ctx)
    {
        if (IsAnyMenuOpen()) return;
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
    /// Callback z ChargeBarManager: pasek osiągnął 100% - przełącz w stan naładowany.
    /// Timing pochodzi z dokładnie tego samego timera co animacja paska - zero desyncu.
    /// </summary>
    void OnChargeBarFull()
    {
        if (IsAnyMenuOpen()) return;
        isPendingHold = false;
        playerShooter.isCharging = true;
        chargingAnimationManager.PlayChargingAnimation();
        audioManager.PlayChargeUpSFX();
    }

    /// <summary>
    /// Callback Input System Hold.performed - pomijany, timing sterowany przez ChargeBarManager.OnChargeFull.
    /// Pozostaje zarejestrowany, by API Input System pozostało nienaruszone.
    /// </summary>
    void OnHoldPerformed(InputAction.CallbackContext ctx)
    {
    }

    /// <summary>
    /// Callback: przycisk zwolniony - wyjście ze stanu ładowania.
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

        if (IsAnyMenuOpen())
        {
            audioManager.StopChargeUpSFX();
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
            audioManager.StopChargeUpSFX();
            playerShooter.FireOnce();
        }
    }
}
