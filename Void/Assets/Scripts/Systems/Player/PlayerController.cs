using UnityEngine;
using UnityEngine.InputSystem;

public abstract class PlayerController : MonoBehaviour
{
    private InputActionMap playerActionMap;

    private void OnEnable()
    {
        playerActionMap ??= InputSystem.actions.FindActionMap("Player");
        playerActionMap.Enable();

        InputAction primaryActionInputAction = playerActionMap.FindAction("Primary Action");
        primaryActionInputAction.performed += OnPrimaryAction;
        primaryActionInputAction.canceled += OnPrimaryAction;

        InputAction secondaryActionInputAction = playerActionMap.FindAction("Secondary Action");
        secondaryActionInputAction.performed += OnSecondaryAction;
        secondaryActionInputAction.canceled += OnSecondaryAction;

        InputAction switchInputAction = playerActionMap.FindAction("Switch");
        switchInputAction.performed += OnSwitch;
    }

    private void OnDisable()
    {
        InputAction primaryActionInputAction = playerActionMap.FindAction("Primary Action");
        primaryActionInputAction.performed -= OnPrimaryAction;
        primaryActionInputAction.canceled -= OnPrimaryAction;

        InputAction secondaryActionInputAction = playerActionMap.FindAction("Secondary Action");
        secondaryActionInputAction.performed -= OnSecondaryAction;
        secondaryActionInputAction.canceled -= OnSecondaryAction;

        InputAction switchInputAction = playerActionMap.FindAction("Switch");
        switchInputAction.performed -= OnSwitch;

        playerActionMap.Disable();
    }

    public abstract void OnPrimaryAction(InputAction.CallbackContext context);
    public abstract void OnSecondaryAction(InputAction.CallbackContext context);
    public abstract void OnSwitch(InputAction.CallbackContext context);
}