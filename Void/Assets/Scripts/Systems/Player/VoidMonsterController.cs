using UnityEngine;
using UnityEngine.InputSystem;

public class VoidMonsterController : PlayerController
{
    private BasicAttack basicAttack;
    private MutationHotbar mutationHotbar;

    public MutationHotbar MutationHotbar => mutationHotbar;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner) UIManager.Instance.SetupUI(GameManager.PlayerRole.Monster, gameObject);
    }

    protected override void Awake()
    {
        base.Awake();
        basicAttack = GetComponent<BasicAttack>();
        mutationHotbar = GetComponent<MutationHotbar>();
    }

    public override void OnDeathStateChanged(Health health, bool isDead)
    {
        base.OnDeathStateChanged(health, isDead);
        if (!IsOwner) return;

        if (isDead)
        {
            UIManager.Instance.TriggerFade(true);
        }
        else
        {
            UIManager.Instance.TriggerFade(false);
            movementController.Teleport(SpawnManager.Instance.GetRandomSpawnpointPosition(Spawnpoint.SpawnpointType.Monster));
        }
    }

    public override void OnPrimaryAction(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (context.performed)
        {
            basicAttack.RequestUse();
        }
        else
        {
            basicAttack.RequestStopUsing();
        }
    }

    public override void OnSecondaryAction(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (context.performed)
        {
            Mutation activeMutation = mutationHotbar.GetMutation();
            if (activeMutation != null) activeMutation.RequestUse();
        }
        else if (context.canceled)
        {
            Mutation activeMutation = mutationHotbar.GetMutation();
            if (activeMutation != null) activeMutation.RequestStopUsing();
        }
    }

    public override void OnSwitch(InputAction.CallbackContext context)
    {
        if (!IsOwner || mutationHotbar.MutationCount <= 0) return;
        int direction = (int)Mathf.Sign(context.ReadValue<float>());
        int newIndex = (mutationHotbar.SelectedIndex + direction + mutationHotbar.MutationCount) % mutationHotbar.MutationCount;
        mutationHotbar.RequestSwitchToMutation(newIndex);
    }

    public override void OnDrop(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        //throw new System.NotImplementedException();
    }
}