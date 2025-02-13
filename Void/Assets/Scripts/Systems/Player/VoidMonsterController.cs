using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class VoidMonsterController : PlayerController
{
    private BasicAttack basicAttack;
    private MutationHotbar mutationHotbar;

    public MutationHotbar MutationHotbar => mutationHotbar;

    protected override void Awake()
    {
        base.Awake();
        basicAttack = GetComponent<BasicAttack>();
        mutationHotbar = GetComponent<MutationHotbar>();
    }

    public override void Die()
    {
        if (!IsServer) return;
        Respawn();
    }

    public override void Respawn()
    {
        health.SetHealth(health.GetMaxHealth());
        movementController.Teleport(SpawnManager.Instance.GetRandomSpawnpointPosition(Spawnpoint.SpawnpointType.Monster));
    }

    public override void OnPrimaryAction(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (context.performed)
        {
            basicAttack.Use();
            //Debug.Log("Tried To Attack");
        }
    }

    public override void OnSecondaryAction(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (context.performed)
        {
            Mutation activeMutation = mutationHotbar.GetActiveMutation();
            if (activeMutation != null) activeMutation.Use();
        }
        else if (context.canceled)
        {
            Mutation activeMutation = mutationHotbar.GetActiveMutation();
            if (activeMutation != null) activeMutation.StopUsing();
        }
    }

    public override void OnSwitch(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        //throw new System.NotImplementedException();
    }

    public override void OnDrop(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        //throw new System.NotImplementedException();
    }
}