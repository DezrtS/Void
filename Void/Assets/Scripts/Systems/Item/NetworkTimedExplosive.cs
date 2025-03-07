using Unity.Netcode;
using UnityEngine;

public class NetworkTimedExplosive : NetworkItem
{
    private TimedExplosive timedExplosive;
    private readonly NetworkVariable<bool> isActive = new();

    protected override void OnItemInitialize()
    {
        base.OnItemInitialize();
        OnGrenadeInitialize();
    }

    private void OnGrenadeInitialize()
    {
        timedExplosive = useable as TimedExplosive;
    }

    protected override void OnItemSpawn()
    {
        base.OnItemSpawn();
        OnGrenadeSpawn();
    }

    private void OnGrenadeSpawn()
    {
        isActive.OnValueChanged += OnActiveStateChanged;
    }

    protected override void OnItemDespawn()
    {
        base.OnItemDespawn();
        OnGrenadeDespawn();
    }

    private void OnGrenadeDespawn()
    {
        isActive.OnValueChanged += OnActiveStateChanged;
    }

    private void OnActiveStateChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue) return;
        if (newValue) timedExplosive.ActivateExplosive();
        else timedExplosive.DeactivateExplosive();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ActivateExplosiveServerRpc()
    {
        if (!timedExplosive.CanActivateExplosive()) return;
        isActive.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DeactivateExplosiveServerRpc()
    {
        if (!timedExplosive.CanDeactivateExplosive()) return;
        isActive.Value = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TriggerExplosiveServerRpc()
    {
        if (!timedExplosive.CanTriggerExplosive()) return;
        TriggerExplosiveClientRpc();
        isActive.Value = false;
    }

    [ClientRpc(RequireOwnership = false)]
    public void TriggerExplosiveClientRpc()
    {
        timedExplosive.TriggerExplosive();
        timedExplosive.gameObject.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TriggerImpactServerRpc()
    {
        TriggerExplosiveClientRpc();
        isActive.Value = false;
    }
}