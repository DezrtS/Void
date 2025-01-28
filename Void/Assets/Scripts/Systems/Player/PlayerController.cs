using System;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class PlayerController : MonoBehaviour
{
    private InputActionMap playerActionMap;
    protected PlayerLook playerLook;

    public event Action<bool> OnSelectionWheel;
    public PlayerLook PlayerLook => playerLook;

    protected virtual void OnEnable()
    {
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
    }

    protected virtual void OnDisable()
    {
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
    }

    protected virtual void Awake()
    {
        playerLook = GetComponent<PlayerLook>();
    }

    public abstract void OnPrimaryAction(InputAction.CallbackContext context);
    public abstract void OnSecondaryAction(InputAction.CallbackContext context);
    public void OnOpenSelection(InputAction.CallbackContext context)
    {
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

    public abstract void OnSwitch(InputAction.CallbackContext context);

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed) playerLook.InteractWithObject();
    }

    public abstract void OnDrop(InputAction.CallbackContext context);
}