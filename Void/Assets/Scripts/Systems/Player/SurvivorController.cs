using UnityEngine;
using UnityEngine.InputSystem;

public class SurvivorController : PlayerController
{
    private Hotbar hotbar;
    private Inventory inventory;

    protected override void Awake()
    {
        base.Awake();
        hotbar = GetComponent<Hotbar>();
        inventory = GetComponent<Inventory>();
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
        int direction = (int)context.ReadValue<float>();
        hotbar.SwitchItem(direction);
    }

    public override void OnDrop(InputAction.CallbackContext context)
    {
        hotbar.DropItem();
    }
}
