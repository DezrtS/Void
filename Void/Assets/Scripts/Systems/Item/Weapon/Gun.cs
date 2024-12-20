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

    protected override void OnUse()
    {
        isFiring = true;
        windupTimer = timeToWindUp;
    }

    protected override void OnStopUsing()
    {
        isFiring = false;
    }

    private void Update()
    {
        UpdateTimers();

        if (isFiring)
        {
            TryFire();
        }
    }

    public virtual bool CanFire()
    {
        return (ammo > 0 && fireRateTimer <= 0 && reloadTimer <= 0 && windupTimer <= 0);
    }

    protected virtual void UpdateTimers()
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

    private void TryFire()
    {
        if (CanFire())
        {
            Fire();
        }
    }

    public void Fire()
    {
        fireRateTimer = fireRate;
        if (!fireSound.IsNull)
        {
            AudioManager.Instance.PlayOneShot(fireSound, transform.position);
        }

        OnFire();
        ammo--;
        if (ammo <= 0)
        {
            Reload();
        }
    }

    protected virtual void OnFire()
    {
        projectileSpawner.SpawnProjectile();
    }

    public void Reload()
    {
        reloadTimer = timeToReload;
        ammo = maxAmmo;
    }
}