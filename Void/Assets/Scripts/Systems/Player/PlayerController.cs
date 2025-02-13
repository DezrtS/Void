using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class PlayerController : NetworkBehaviour
{
    private InputActionMap playerActionMap;
    private InputActionMap uiActionMap;

    protected MovementController movementController;
    protected PlayerLook playerLook;
    protected Health health;

    public event Action<bool> OnSelectionWheel;
    public PlayerLook PlayerLook => playerLook;

    protected virtual void OnEnable()
    {
        uiActionMap ??= InputSystem.actions.FindActionMap("UI");
        uiActionMap.Enable();

        InputAction pauseActionInputAction = uiActionMap.FindAction("Pause");
        pauseActionInputAction.performed += OnPause;

        playerActionMap ??= InputSystem.actions.FindActionMap("Player");
        playerActionMap.Enable();

        InputAction primaryActionInputAction = playerActionMap.FindAction("Primary Action");
        primaryActionInputAction.performed += OnPrimaryAction;
        primaryActionInputAction.canceled += OnPrimaryAction;

        InputAction secondaryActionInputAction = playerActionMap.FindAction("Secondary Action");
        secondaryActionInputAction.performed += OnSecondaryAction;
        secondaryActionInputAction.canceled += OnSecondaryAction;

        InputAction openSelectionInputAction = playerActionMap.FindAction("Open Selection");
        openSelectionInputAction.performed += OnOpenSelection;
        openSelectionInputAction.canceled += OnOpenSelection;

        InputAction switchInputAction = playerActionMap.FindAction("Switch");
        switchInputAction.performed += OnSwitch;

        InputAction interactInputAction = playerActionMap.FindAction("Interact");
        interactInputAction.performed += OnInteract;

        InputAction dropInputAction = playerActionMap.FindAction("Drop");
        dropInputAction.performed += OnDrop;

        UIManager.OnPause += (bool paused) =>
        {
            if (paused)
            {
                playerActionMap.Disable();
            }
            else
            {
                playerActionMap.Enable();
            }
        };
    }

    protected virtual void OnDisable()
    {
        InputAction pauseActionInputAction = uiActionMap.FindAction("Pause");
        pauseActionInputAction.performed -= OnPause;

        InputAction primaryActionInputAction = playerActionMap.FindAction("Primary Action");
        primaryActionInputAction.performed -= OnPrimaryAction;
        primaryActionInputAction.canceled -= OnPrimaryAction;

        InputAction secondaryActionInputAction = playerActionMap.FindAction("Secondary Action");
        secondaryActionInputAction.performed -= OnSecondaryAction;
        secondaryActionInputAction.canceled -= OnSecondaryAction;

        InputAction openSelectionInputAction = playerActionMap.FindAction("Open Selection");
        openSelectionInputAction.performed -= OnOpenSelection;
        openSelectionInputAction.canceled -= OnOpenSelection;

        InputAction switchInputAction = playerActionMap.FindAction("Switch");
        switchInputAction.performed -= OnSwitch;

        InputAction interactInputAction = playerActionMap.FindAction("Interact");
        interactInputAction.performed -= OnInteract;

        InputAction dropInputAction = playerActionMap.FindAction("Drop");
        dropInputAction.performed -= OnDrop;

        playerActionMap.Disable();
        uiActionMap.Disable();
    }

    protected virtual void Awake()
    {
        movementController = GetComponent<MovementController>();
        playerLook = GetComponent<PlayerLook>();
        health = GetComponent<Health>();
        health.OnDeath += (Health health) =>
        {
            Die();
        };
    }

    public abstract void Die();
    public abstract void Respawn();

    public abstract void OnPrimaryAction(InputAction.CallbackContext context);
    public abstract void OnSecondaryAction(InputAction.CallbackContext context);
    public void OnOpenSelection(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (context.performed)
        {
            OnSelectionWheel?.Invoke(true);
            return;
        }

        if (context.canceled)
        {
            OnSelectionWheel?.Invoke(false);
            return;
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        UIManager.Instance.PauseUnpauseGame();
    }

    public abstract void OnSwitch(InputAction.CallbackContext context);

    // May be being run by the server
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if (context.performed) playerLook.InteractWithObject();
    }

    public abstract void OnDrop(InputAction.CallbackContext context);
}