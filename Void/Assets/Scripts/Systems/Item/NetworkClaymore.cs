using Unity.Netcode;

public class NetworkClaymore : NetworkDeployableItem
{
    private Claymore claymore;
    private readonly NetworkVariable<bool> isActive = new();

    protected override void OnDeployableItemInitialize()
    {
        base.OnDeployableItemInitialize();
        OnClaymoreInitialize();
    }

    protected void OnClaymoreInitialize()
    {
        claymore = useable as Claymore;
    }

    protected override void OnDeployableItemSpawn()
    {
        base.OnDeployableItemSpawn();
        OnClaymoreSpawn();
    }

    protected void OnClaymoreSpawn()
    {
        isActive.OnValueChanged += OnActiveStateChanged;
    }

    protected override void OnDeployableItemDespawn()
    {
        base.OnDeployableItemDespawn();
        OnClaymoreDespawn();
    }

    protected void OnClaymoreDespawn()
    {
        isActive.OnValueChanged -= OnActiveStateChanged;
    }

    private void OnActiveStateChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue) return;
        if (newValue) claymore.ActivateClaymore();
        else claymore.DeactivateClaymore();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ActivateClaymoreServerRpc()
    {
        if (!claymore.CanActivateClaymore()) return;
        isActive.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DeactivateClaymoreServerRpc()
    {
        if (!claymore.CanDeactivateClaymore()) return;
        isActive.Value = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TriggerClaymoreServerRpc()
    {
        TriggerClaymoreClientRpc();
    }

    [ClientRpc(RequireOwnership = false)]
    public void TriggerClaymoreClientRpc()
    {
        claymore.TriggerClaymore();
        claymore.gameObject.SetActive(false);
    }
}