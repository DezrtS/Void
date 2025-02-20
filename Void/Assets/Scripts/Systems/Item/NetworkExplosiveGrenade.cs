using Unity.Netcode;

public class NetworkExplosiveGrenade : NetworkItem
{
    private ExplosiveGrenade explosiveGrenade;
    private readonly NetworkVariable<bool> isActive = new();

    protected override void OnItemInitialize()
    {
        base.OnItemInitialize();
        OnGrenadeInitialize();
    }

    private void OnGrenadeInitialize()
    {
        explosiveGrenade = useable as ExplosiveGrenade;
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
        if (newValue) explosiveGrenade.ActivateGrenade();
        else explosiveGrenade.DeactivateGrenade();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ActivateGrenadeServerRpc()
    {
        if (!explosiveGrenade.CanActivateGrenade()) return;
        isActive.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DeactivateGrenadeServerRpc()
    {
        if (!explosiveGrenade.CanDeactivateGrenade()) return;
        isActive.Value = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TriggerGrenadeServerRpc()
    {
        if (!explosiveGrenade.CanTriggerGrenade()) return;
        TriggerGrenadeClientRpc();
    }

    [ClientRpc(RequireOwnership = false)]
    public void TriggerGrenadeClientRpc()
    {
        explosiveGrenade.TriggerGrenade();
        explosiveGrenade.gameObject.SetActive(false);
    }
}