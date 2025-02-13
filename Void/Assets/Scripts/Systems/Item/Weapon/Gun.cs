using FMODUnity;
using Unity.Netcode;
using UnityEngine;

public class Gun : Item, IReload
{
    [SerializeField] private GameObject projectileSpawnerObject;
    [SerializeField] private float fireRate;
    [SerializeField] private int maxAmmo;
    [SerializeField] private float timeToReload;
    [SerializeField] private float timeToWindUp;

    [SerializeField] private EventReference fireSound;
    [SerializeField] private EventReference emptySound;
    [SerializeField] private EventReference reloadSound;

    protected IProjectileSpawner projectileSpawner;
    [SerializeField] protected bool isFiring;
    protected int ammo;
    private float fireRateTimer;
    private float reloadTimer;
    private float windupTimer;

    public int MaxAmmo => maxAmmo;
    public int Ammo => ammo;
    public float ReloadSpeed => timeToReload;

    private void Start()
    {
        projectileSpawner = projectileSpawnerObject.GetComponent<IProjectileSpawner>();
        ammo = maxAmmo;
    }

    protected override void UseClientServerSide()
    {
        base.UseClientServerSide();
        isFiring = true;
        windupTimer = timeToWindUp;
    }

    protected override void StopUsingClientServerSide()
    {
        base.StopUsingClientServerSide();
        isFiring = false;
    }

    public bool HasAmmo()
    {
        return ammo > 0;
    }

    public bool CanFire()
    {
        return (fireRateTimer <= 0 && reloadTimer <= 0 && windupTimer <= 0);
    }

    private void UpdateTimers()
    {
        float deltaTime = Time.deltaTime;
        if (fireRateTimer > 0)
        {
            fireRateTimer -= deltaTime;
        }

        if (reloadTimer > 0)
        {
            reloadTimer -= deltaTime;
        }

        if (windupTimer > 0)
        {
            windupTimer -= deltaTime;
        }
    }

    private void Update()
    {
        UpdateTimers();

        if (!IsOwner) return;

        if (isFiring)
        {
            Fire();
        }
    }

    public void Fire()
    {
        if (!CanFire()) return;
        ForceFire();
    }

    public void ForceFire()
    {
        if (!IsServer) FireClientSide();
        FireServerRpc();
    }

    private void FireClientSide()
    {
        if (!HasAmmo())
        {
            AudioManager.Instance.PlayOneShot(emptySound, transform.position);
            if (IsOwner) StopUsing();
            return;
        }

        FireClientServerSide();
        AudioManager.Instance.PlayOneShot(fireSound, transform.position);
    }

    private void FireServerSide()
    {
        if (!HasAmmo()) return;

        FireClientServerSide();
    }

    private void FireClientServerSide()
    {
        fireRateTimer = fireRate;
        ammo--;
        projectileSpawner.SpawnProjectile();
    }

    [ServerRpc(RequireOwnership = false)]
    private void FireServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!CanFire()) return;
        FireServerSide();
        //ClientRpcParams clientRpcParams = GameMultiplayer.GenerateClientRpcParams(rpcParams);
        FireClientRpc();
    }

    [ClientRpc(RequireOwnership = false)]
    private void FireClientRpc(ClientRpcParams rpcParams = default)
    {
        FireClientSide();
    }

    public bool CanReload()
    {
        return reloadTimer <= 0;
    }

    public void Reload()
    {
        if (!CanReload()) return;
        ForceReload();
    }

    public void ForceReload()
    {
        if (!IsServer) ReloadClientSide();
        ReloadServerRpc();
    }

    private void ReloadClientSide()
    {
        AudioManager.Instance.PlayOneShot(reloadSound, transform.position);
        ReloadClientServerSide();
    }

    private void ReloadServerSide()
    {
        ReloadClientServerSide();
    }

    private void ReloadClientServerSide()
    {
        reloadTimer = timeToReload;
        ammo = maxAmmo;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReloadServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!CanReload()) return;
        ReloadServerSide();
        //ClientRpcParams clientRpcParams = GameMultiplayer.GenerateClientRpcParams(rpcParams);
        ReloadClientRpc();
    }

    [ClientRpc(RequireOwnership = false)]
    private void ReloadClientRpc(ClientRpcParams rpcParams = default)
    {
        ReloadClientSide();
    }
}