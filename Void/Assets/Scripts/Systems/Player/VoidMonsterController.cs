using UnityEngine;
using UnityEngine.InputSystem;

public class VoidMonsterController : PlayerController
{
    private BasicAttack basicAttack;

    private void Awake()
    {
        basicAttack = GetComponent<BasicAttack>();
    }

    public override void OnPrimaryAction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            basicAttack.Use();
            //Debug.Log("Tried To Attack");
        }
    }

    public override void OnSecondaryAction(InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public override void OnSwitch(InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }
}