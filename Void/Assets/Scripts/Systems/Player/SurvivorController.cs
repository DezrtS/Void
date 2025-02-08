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

        if (TryGetComponent(out Health health)) health.OnDeath += Die;
    }

    public override void Die(Health health)
    {
        health.SetHealth(health.GetMaxHealth());
        hotbar.DropAllItems();
        PlayerSpawnPoint playerSpawnPoint = GameManager.Instance.GetAvailablePlayerSpawnPoint(GameManager.PlayerRole.Survivor);
        Vector3 spawnPosition = Vector3.zero;
        if (playerSpawnPoint != null) spawnPosition = playerSpawnPoint.transform.position;
        transform.position = spawnPosition;
    }

    public override void OnPrimaryAction(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (context.performed)
        {
            Item activeItem = hotbar.GetItem();
            if (activeItem != null) activeItem.Use();
        }
        else if (context.canceled)
        {
            Item activeItem = hotbar.GetItem();
            if (activeItem != null) activeItem.StopUsing();
        }
    }

    public override void OnSecondaryAction(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        //throw new System.NotImplementedException();
    }

    public override void OnSwitch(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        int direction = (int)Mathf.Sign(context.ReadValue<float>());
        hotbar.SwitchItem(direction);
    }

    public override void OnDrop(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (hotbar.IsDragging)
        {
            hotbar.StopDragging();
        }
        else
        {
            hotbar.DropItem();
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (context.performed)
        {
            Item activeItem = hotbar.GetItem();
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
