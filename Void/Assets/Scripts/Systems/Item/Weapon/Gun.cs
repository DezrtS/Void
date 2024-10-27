using FMODUnity;
using UnityEngine;

public class Gun : Item
{
    [SerializeField] private GameObject projectileSpawnerObject;
    [SerializeField] private float fireRate;
    [SerializeField] private int maxAmmo;
    [SerializeField] private float reloadSpeed;
    [SerializeField] private float windupTime;
    [SerializeField] private EventReference fireSound;

    protected IProjectileSpawner projectileSpawner;
    protected bool firing;
    protected int ammo;
    private float fireRateTimer;
    private float reloadTimer;
    private float windupTimer;

    private void Start()
    {
        projectileSpawner = projectileSpawnerObject.GetComponent<IProjectileSpawner>();
        ammo = maxAmmo;
    }

    protected override void OnUse()
    {
        firing = true;
        windupTimer = windupTime;
    }

    protected override void OnStopUsing()
    {
        firing = false;
    }

    private void FixedUpdate()
    {
        UpdateTimers();

        if (firing)
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
        if (fireRateTimer > 0)
        {
            fireRateTimer -= Time.fixedDeltaTime;
        }

        if (reloadTimer > 0)
        {
            reloadTimer -= Time.fixedDeltaTime;
        }

        if (windupTimer > 0)
        {
            windupTimer -= Time.fixedDeltaTime;
        }
    }

    private void TryFire()
    {
        if (CanFire())
        {
            Fire();
            fireRateTimer = fireRate;
        }
    }

    public void Fire()
    {
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

    protected void Reload()
    {
        reloadTimer = reloadSpeed;
        ammo = maxAmmo;
    }
}