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
        if (networkClaymore.IsServer) RequestActivateClaymore();
        base.Deploy();
    }

    public override void Undeploy()
    {
        base.Undeploy();
        if (networkClaymore.IsServer) RequestDeactivateClaymore();
    }

    public void OnEnter(Trigger trigger, GameObject gameObject)
    {
        if (!gameObject.CompareTag("Player") && !gameObject.CompareTag("Monster")) return;
        RequestTriggerClaymore();

        if (gameObject.TryGetComponent(out Health health))
        {
            trigger.OnEnter -= OnEnter;
            health.RequestDamage(damage);
        }
    }

    public void ActivateClaymore()
    {
        isActive = true;
        if (networkClaymore.IsServer) trigger.OnEnter += OnEnter;
    }

    public void DeactivateClaymore()
    {
        isActive = false;
        if (networkClaymore.IsServer) trigger.OnEnter -= OnEnter;
    }

    public void TriggerClaymore()
    {
        AudioManager.PlayOneShot(activateSound, gameObject);
        Instantiate(explosionEffect, transform.position, transform.rotation);
    }
}