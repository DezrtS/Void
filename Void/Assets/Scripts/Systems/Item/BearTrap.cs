using FMODUnity;
using UnityEngine;

public class BearTrap : DeployableItem
{
    [SerializeField] private Trigger trigger;
    [SerializeField] private Animator animator;
    [SerializeField] private float initialDamage;
    [SerializeField] private float damage;
    [SerializeField] private float duration;
    [SerializeField] private EventReference activateSound;
    
    private NetworkBearTrap networkBearTrap;
    private bool isPrimed;
    private bool isActive;

    private Health captured;
    private float durationTimer;

    public bool CanPrimeBearTrap() => !isPrimed;
    public bool CanUnprimeBearTrap() => isPrimed;
    public bool CanActivateBearTrap() => !isActive;
    public bool CanDeactivateBearTrap() => isActive;

    protected override void OnDeployableItemInitialize()
    {
        base.OnDeployableItemInitialize();
        OnBearTrapInitialize();
    }

    protected void OnBearTrapInitialize()
    {
        networkBearTrap = NetworkItem as NetworkBearTrap;
    }

    private void FixedUpdate()
    {
        if (durationTimer > 0)
        {
            //if (networkBearTrap.IsServer) captured.RequestDamage(damage * Time.fixedDeltaTime);
            durationTimer -= Time.fixedDeltaTime;
            if (durationTimer <= 0)
            {
                if (networkBearTrap.IsServer) RequestDeactivateBearTrap();
            }
        }
    }

    public void RequestPrimeBearTrap() => networkBearTrap.PrimeBearTrapServerRpc();
    public void RequestUnprimeBearTrap() => networkBearTrap.UnprimeBearTrapServerRpc();
    public void RequestActivateBearTrap() => networkBearTrap.ActivateBearTrapServerRpc();
    public void RequestDeactivateBearTrap() => networkBearTrap.DeactivateBearTrapServerRpc();

    public override void Deploy()
    {
        base.Deploy();
        if (networkBearTrap.IsOwner) RequestPrimeBearTrap();
    }

    public override void Undeploy()
    {
        base.Undeploy();
        if (networkBearTrap.IsOwner) RequestUnprimeBearTrap();
    }

    public void OnEnter(Trigger trigger, GameObject gameObject)
    {
        if (captured != null) return;

        if (!GameManager.CanDamage(gameObject, GameManager.PlayerRole.Survivor)) return;
        if (gameObject.TryGetComponent(out captured))
        {
            trigger.OnEnter -= OnEnter;
            if (networkBearTrap.IsOwner) UIManager.Instance.TriggerHit();
            if (networkBearTrap.IsServer)
            {
                captured.RequestDamage(initialDamage);
                if (captured.gameObject.TryGetComponent(out MovementController movementController))
                {
                    movementController.RequestSetMovementDisabled(true);
                }
                RequestActivateBearTrap();
            }
        }
    }

    public void PrimeBearTrap()
    {
        isPrimed = true;
        animator.SetBool("Active", isActive);
        trigger.OnEnter += OnEnter;
    }

    public void UnprimeBearTrap()
    {
        isPrimed = false;
        animator.SetBool("Active", true);
        trigger.OnEnter -= OnEnter;
    }

    public void ActivateBearTrap()
    {
        isActive = true;
        canPickUp = false;
        durationTimer = duration;
        AudioManager.PlayOneShot(activateSound, gameObject);
        animator.SetBool("Active", isActive);
    }

    public void DeactivateBearTrap()
    {
        isActive = false;
        canPickUp = true;
        durationTimer = 0;
        if (networkBearTrap.IsServer)
        {
            if (captured.gameObject.TryGetComponent(out MovementController movementController))
            {
                movementController.RequestSetMovementDisabled(false);
            }
        }
        captured = null;
    }
}