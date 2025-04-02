using FMODUnity;
using UnityEngine;

public class Claymore : DeployableItem
{
    [SerializeField] private Trigger trigger;
    [SerializeField] private float damage;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private EventReference activateSound;

    private NetworkClaymore networkClaymore;
    private bool isActive;

    public bool CanActivateClaymore() => !isActive;
    public bool CanDeactivateClaymore() => isActive;

    protected override void OnDeployableItemInitialize()
    {
        base.OnDeployableItemInitialize();
        OnClaymoreInitialize();
    }

    protected void OnClaymoreInitialize()
    {
        networkClaymore = NetworkItem as NetworkClaymore;
    }

    public void RequestActivateClaymore() => networkClaymore.ActivateClaymoreServerRpc();
    public void RequestDeactivateClaymore() => networkClaymore.DeactivateClaymoreServerRpc();
    public void RequestTriggerClaymore() => networkClaymore.TriggerClaymoreServerRpc();

    public override void Deploy()
    {
        base.Deploy();
        if (networkClaymore.IsOwner) RequestActivateClaymore();
    }

    public override void Undeploy()
    {
        base.Undeploy();
        if (networkClaymore.IsOwner) RequestDeactivateClaymore();
    }

    public void OnEnter(Trigger trigger, GameObject gameObject)
    {
        if (!gameObject.CompareTag("Player") && !gameObject.CompareTag("Monster")) return;
        if (!GameManager.CanDamage(gameObject, GameManager.PlayerRole.Survivor)) return;
        if (networkClaymore.IsServer) RequestTriggerClaymore();

        if (gameObject.TryGetComponent(out Health health))
        {
            trigger.OnEnter -= OnEnter;
            if (networkClaymore.IsOwner) UIManager.Instance.TriggerHit();
            if (networkClaymore.IsServer) health.RequestDamage(damage);
        }
    }

    public void ActivateClaymore()
    {
        isActive = true;
        trigger.OnEnter += OnEnter;
    }

    public void DeactivateClaymore()
    {
        isActive = false;
        trigger.OnEnter -= OnEnter;
    }

    public void TriggerClaymore()
    {
        AudioManager.PlayOneShot(activateSound, gameObject);
        Instantiate(explosionEffect, transform.position, transform.rotation);
    }
}