using Unity.Netcode;

public class NetworkBearTrap : NetworkDeployableItem
{
    private BearTrap bearTrap;
    private readonly NetworkVariable<bool> isActive = new();

    protected override void OnDeployableItemInitialize()
    {
        base.OnDeployableItemInitialize();
        OnBearTrapInitialize();
    }

    protected void OnBearTrapInitialize()
    {
        bearTrap = useable as BearTrap;
    }

    protected override void OnDeployableItemSpawn()
    {
        base.OnDeployableItemSpawn();
        OnBearTrapSpawn();
    }

    protected void OnBearTrapSpawn()
    {
        isActive.OnValueChanged += OnActiveStateChanged;
    }

    protected override void OnDeployableItemDespawn()
    {
        base.OnDeployableItemDespawn();
        OnBearTrapDespawn();
    }

    protected void OnBearTrapDespawn()
    {
        isActive.OnValueChanged -= OnActiveStateChanged;
    }

    private void OnActiveStateChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue) return;
        if (newValue) bearTrap.ActivateBearTrap();
        else bearTrap.DeactivateBearTrap();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ActivateBearTrapServerRpc()
    {
        if (!bearTrap.CanActivateBearTrap()) return;
        isActive.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DeactivateBearTrapServerRpc()
    {
        if (!bearTrap.CanDeactivateBearTrap()) return;
        isActive.Value = false;
    }
}
