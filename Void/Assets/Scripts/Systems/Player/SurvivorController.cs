using UnityEngine;
using UnityEngine.InputSystem;

public class SurvivorController : PlayerController
{
    private Inventory inventory;

    private void Awake()
    {
        inventory = GetComponent<Inventory>();
    }

    public override void OnPrimaryAction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            inventory.ActiveHotbarSlot()?.WorldItem.Use();
        }
        else if (context.canceled)
        {
            inventory.ActiveHotbarSlot()?.WorldItem.StopUsing();
        }
    }

    public override void OnSecondaryAction(InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public override void OnSwitch(InputAction.CallbackContext context)
    {
        int direction = (int)context.ReadValue<float>();
        inventory.SwitchItem(direction);
    }
}
