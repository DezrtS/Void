using UnityEngine;
using UnityEngine.InputSystem;

public class SurvivorController : PlayerController
{
    private Hotbar hotbar;
    private Inventory inventory;

    private InputActionMap survivorActionMap;

    public Hotbar Hotbar => hotbar;
    public Inventory Inventory => inventory;

    protected override void OnEnable()
    {
        //if (!IsOwner) return;
        base.OnEnable();

        survivorActionMap ??= InputSystem.actions.FindActionMap("Survivor");
        survivorActionMap.Enable();

        InputAction switchInputAction = survivorActionMap.FindAction("Reload");
        switchInputAction.performed += OnReload;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        InputAction switchInputAction = survivorActionMap.FindAction("Reload");
        switchInputAction.performed -= OnReload;

        survivorActionMap.Disable();
    }

    protected override void Awake()
    {
        base.Awake();
        hotbar = GetComponent<Hotbar>();
        inventory = GetComponent<Inventory>();

        if (TryGetComponent(out Health health)) health.OnDeath += hotbar.DropAllItems;
    }

    public override void OnPrimaryAction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Item activeItem = hotbar.GetActiveItem();
            if (activeItem != null) activeItem.Use();
        }
        else if (context.canceled)
        {
            Item activeItem = hotbar.GetActiveItem();
            if (activeItem != null) activeItem.StopUsing();
        }
    }

    public override void OnSecondaryAction(InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public override void OnSwitch(InputAction.CallbackContext context)
    {
        int direction = (int)Mathf.Sign(context.ReadValue<float>());
        hotbar.SwitchItem(direction);
    }

    public override void OnDrop(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        hotbar.DropItem();
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Item activeItem = hotbar.GetActiveItem();
            if (activeItem != null)
            {
                if (activeItem.TryGetComponent(out IReload reload))
                {
                    reload.Reload();
                }
                
            }
        }
    }
}
