using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Item, IReload, IAnimate
{
    public event IAnimate.AnimationEventHandler OnAnimationEvent;
    public event Action OnFire;

    [SerializeField] private GameObject projectileSpawnerObject;
    [SerializeField] private float fireRate;
    [SerializeField] private int maxAmmo;
    [SerializeField] private float timeToReload;
    [SerializeField] private float timeToWindUp;

    [SerializeField] private EventReference fireSound;
    [SerializeField] private EventReference emptySound;
    [SerializeField] private EventReference reloadSound;

    private NetworkGun networkGun;
    private bool isFiring;
    private int ammo;

    private IProjectileSpawner projectileSpawner;
    private float fireRateTimer;
    private float reloadTimer;
    private float windupTimer;

    private Recoil recoil;

    public int MaxAmmo => maxAmmo;
    public int Ammo { get { return ammo; } set { ammo = value; } }

    public bool CanStartFire() => !isFiring;
    public bool CanStopFiring() => isFiring;
    public bool CanFire() => fireRateTimer <= 0 && reloadTimer <= 0 && windupTimer <= 0;
    public bool CanReload() => reloadTimer <= 0;

    public void RequestStartFiring() => networkGun.StartFiringServerRpc();
    public void RequestStopFiring() => networkGun.StopFiringServerRpc();
    public void RequestFire() => networkGun.FireServerRpc();
    public void RequestReload() => networkGun.ReloadServerRpc();

    private void Start()
    {
        networkGun = NetworkItem as NetworkGun;
        projectileSpawner = projectileSpawnerObject.GetComponent<IProjectileSpawner>();
        recoil = GetComponent<Recoil>();  
    }

    private void Update()
    {
        UpdateTimers();

        if (networkGun.IsServer && isFiring)
        {
            RequestFire();
        }
    }

    public override void Use()
    {
        base.Use();
        if (networkGun.IsServer) RequestStartFiring();
    }

    public override void StopUsing()
    {
        base.StopUsing();
        if (networkGun.IsServer) RequestStopFiring();
    }

    public void StartFiring()
    {
        isFiring = true;
        windupTimer = timeToWindUp;
    }

    public void StopFiring()
    {
        isFiring = false;
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

    public void Fire()
    {
        if (ammo <= 0)
        {
            AudioManager.PlayOneShot(emptySound, gameObject);
            return;
        }

        fireRateTimer = fireRate;
        projectileSpawner.SpawnProjectile();
        //recoil.ApplyRecoil();
        OnFire?.Invoke();
        OnAnimationEvent?.Invoke(IAnimate.AnimationEventType.Trigger, "Fire", null);
        AudioManager.PlayOneShot(fireSound, gameObject);
    }

    public void Reload()
    {
        AudioManager.PlayOneShot(reloadSound, gameObject);
        reloadTimer = timeToReload;
    }

    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);

    }
}