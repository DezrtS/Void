using UnityEngine;
using UnityEngine.InputSystem;

public class VoidMonsterController : PlayerController
{
    private BasicAttack basicAttack;
    private MutationHotbar mutationHotbar;
    private AnimationController animationController;

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
        animationController = GetComponent<AnimationController>();
    }

    public override void OnDeathStateChanged(Health health, bool isDead)
    {
        base.OnDeathStateChanged(health, isDead);
        if (!IsOwner) return;

        if (isDead)
        {
            animationController.SetTrigger("Die");
            animationController.SetTrigger("DisableIK");
            playerLook.EnableDisableCameraControls(false);
            UIManager.Instance.TriggerFade(true);
        }
        else
        {
            animationController.SetTrigger("Respawn");
            animationController.SetTrigger("EnableIK");
            playerLook.EnableDisableCameraControls(true);
            UIManager.Instance.TriggerFade(false);
            health.RequestFullHeal();
            movementController.Teleport(SpawnManager.Instance.GetRandomSpawnpointPosition(Spawnpoint.SpawnpointType.Monster));
        }
    }

    public override void OnPrimaryAction(InputAction.CallbackContext context)
    {
        if (!IsOwner || health.IsDead) return;

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
        if (!IsOwner || health.IsDead) return;

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
        if (!IsOwner || health.IsDead) return;

        int direction = (int)Mathf.Sign(context.ReadValue<float>());
        int newIndex = (mutationHotbar.SelectedIndex + direction + mutationHotbar.MutationCount) % mutationHotbar.MutationCount;
        mutationHotbar.RequestSwitchToMutation(newIndex);
    }

    public override void OnDrop(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        //throw new System.NotImplementedException();
    }

    public override void OnHotbarSlot(InputAction.CallbackContext context)
    {
        if (!IsOwner || health.IsDead) return;

        int index = (int)context.ReadValue<float>() - 1;
        mutationHotbar.RequestSwitchToMutation(index);
    }
}