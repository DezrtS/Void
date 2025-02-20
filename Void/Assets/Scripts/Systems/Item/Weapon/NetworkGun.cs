using Unity.Netcode;

public class NetworkGun : NetworkItem
{
    private Gun gun;
    private readonly NetworkVariable<bool> isFiring = new();
    private readonly NetworkVariable<int> ammo = new();

    protected override void OnItemInitialize()
    {
        base.OnItemInitialize();
        OnGunInitialize();
    }

    private void OnGunInitialize()
    {
        gun = useable as Gun;
    }

    protected override void OnItemSpawn()
    {
        base.OnItemSpawn();
        OnGunSpawn();
    }

    private void OnGunSpawn()
    {
        isFiring.OnValueChanged += OnFiringStateChanged;
        ammo.OnValueChanged += OnAmmoCountChanged;

        if (IsServer) ammo.Value = gun.MaxAmmo;
    }

    protected override void OnItemDespawn()
    {
        base.OnItemDespawn();
        OnGunDespawn();
    }

    private void OnGunDespawn()
    {
        isFiring.OnValueChanged -= OnFiringStateChanged;
        ammo.OnValueChanged -= OnAmmoCountChanged;
    }

    private void OnFiringStateChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue) return;
        if (newValue) gun.StartFiring();
        else gun.StopFiring();
    }

    private void OnAmmoCountChanged(int oldValue, int newValue)
    {
        if (oldValue == newValue) return;
        gun.Ammo = newValue;
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartFiringServerRpc()
    {
        if (!gun.CanStartFire()) return;
        isFiring.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopFiringServerRpc()
    {
        if (!gun.CanStopFiring()) return;
        isFiring.Value = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void FireServerRpc()
    {
        if (!gun.CanFire()) return;
        FireClientRpc();

        if (ammo.Value <= 0) StopFiringServerRpc();
        else ammo.Value--;
    }

    [ClientRpc(RequireOwnership = false)]
    private void FireClientRpc()
    {
        gun.Fire();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReloadServerRpc()
    {
        if (!gun.CanReload()) return;
        ReloadClientRpc();
        ammo.Value = gun.MaxAmmo;
    }

    [ClientRpc(RequireOwnership = false)]
    private void ReloadClientRpc()
    {
        gun.Reload();
    }
}