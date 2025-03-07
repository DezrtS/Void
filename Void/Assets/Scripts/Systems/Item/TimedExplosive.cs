using System.Collections.Generic;
using UnityEngine;

public class TimedExplosive : ThrowableItem
{
    private TimedExplosiveItemData timedExplosiveItemData;

    private NetworkTimedExplosive networkTimedExplosive;
    private bool isActive;

    private float timer = 0;

    public bool CanActivateExplosive() => !isActive;
    public bool CanDeactivateExplosive() => isActive;
    public bool CanTriggerExplosive() => timer <= 0;

    public void RequestActivateExplosive() => networkTimedExplosive.ActivateExplosiveServerRpc();
    public void RequestDeactivateExplosive() => networkTimedExplosive.DeactivateExplosiveServerRpc();
    public void RequestTriggerExplosive() => networkTimedExplosive.TriggerExplosiveServerRpc();
    public void RequestTriggerImpact() => networkTimedExplosive.TriggerImpactServerRpc();

    protected override void Start()
    {
        base.Start();
        timedExplosiveItemData = ItemData as TimedExplosiveItemData;
        networkTimedExplosive = NetworkItem as NetworkTimedExplosive;
    }

    private void FixedUpdate()
    {
        UpdateTimers(Time.fixedDeltaTime);

        if (networkTimedExplosive.IsServer && isActive)
        {
            RequestTriggerExplosive();
        }
    }

    public override void Throw()
    {
        base.Throw();
        if (networkTimedExplosive.IsServer) RequestActivateExplosive();
    }

    public void ActivateExplosive() 
    {
        isActive = true;
        timer = timedExplosiveItemData.TimeToActivation;
    }

    public void DeactivateExplosive()
    {
        isActive = false;
        timer = 0;
    }

    public void TriggerExplosive()
    {
        Instantiate(timedExplosiveItemData.ExplosionEffect, transform.position, Quaternion.identity);

        if (NetworkItem.IsServer)
        {
            RaycastHit[] raycastHits = new RaycastHit[10];
            Physics.SphereCastNonAlloc(transform.position, timedExplosiveItemData.Radius, Vector3.forward, raycastHits, timedExplosiveItemData.Radius, timedExplosiveItemData.EffectableLayers);
            OnExplosiveHit(raycastHits);
        }
    }

    public virtual void OnExplosiveHit(RaycastHit raycastHit)
    {
        if (raycastHit.collider == null) return;

        if (raycastHit.collider.TryGetComponent(out Health health))
        {
            health.RequestDamage(timedExplosiveItemData.Damage);
        }
    }

    public virtual void OnExplosiveHit(IEnumerable<RaycastHit> raycastHits)
    {
        foreach (RaycastHit hit in raycastHits)
        {
            OnExplosiveHit(hit);
        }
    }

    private void UpdateTimers(float deltaTime)
    {
        if (timer > 0)
        {
            timer -= deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (networkTimedExplosive.IsServer && timedExplosiveItemData.ExplodeOnImpact)
        {
            RequestTriggerImpact();
        }
    }
}
