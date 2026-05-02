using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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
    bool isPendingHold;
    Coroutine chargeBarCoroutine;

    void Awake()
    {
        // playerShooter = GetComponent<Shooter>();
        fireAction = InputSystem.actions.FindAction("Fire");
        moveAction = InputSystem.actions.FindAction("Move");
        chargingAnimationManager = FindFirstObjectByType<ChargingAnimationManager>();
        audioManager = FindFirstObjectByType<AudioManager>();
        chargeBarManager = FindFirstObjectByType<ChargeBarManager>();
    }

    void Start()
    {
        InitializeBound();
    }

    void InitializeBound()
    {
        Camera mainCamera = Camera.main;
        minBound = mainCamera.ViewportToWorldPoint(new Vector2(0, 0));
        maxBound = mainCamera.ViewportToWorldPoint(new Vector2(1, 1));
    }

    void Update()
    {
        MovePlayer();
        FireShooter();
    }

    void MovePlayer()
    {
        moveVector = moveAction.ReadValue<Vector2>();
        Vector3 newPos = transform.position + (moveVector * moveSpeed * Time.deltaTime);

        newPos.x = Math.Clamp(newPos.x, minBound.x + leftBoundPadding, maxBound.x - rightBoundPadding);
        newPos.y = Math.Clamp(newPos.y, minBound.y + bottomBoundPadding, maxBound.y - topBoundPadding);

        transform.position = newPos;
    }

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

    void OnEnable()
    {
        fireAction.Enable();

        fireAction.started += OnHoldStarted;
        fireAction.performed += OnHoldPerformed;
        fireAction.canceled += OnHoldCanceled;
    }

    void OnDisable()
    {
        fireAction.started -= OnHoldStarted;
        fireAction.performed -= OnHoldPerformed;
        fireAction.canceled -= OnHoldCanceled;

        fireAction.Disable();
    }

    void OnHoldStarted(InputAction.CallbackContext ctx)
    {
        if (MusicController.IsOpen) return;
        isPendingHold = true;
        chargeBarCoroutine = StartCoroutine(ShowChargeBarDelayed(0.2f));
    }

    IEnumerator ShowChargeBarDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        chargeBarManager.PlayChargeBarAnimation();
    }

    void OnHoldPerformed(InputAction.CallbackContext ctx)
    {
        if (MusicController.IsOpen) return;
        isPendingHold = false;
        playerShooter.isCharging = true;
        chargingAnimationManager.PlayChargingAnimation();
    }

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
            // Reset stan bez strzelania
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
