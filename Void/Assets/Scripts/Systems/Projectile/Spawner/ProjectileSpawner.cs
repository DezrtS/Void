using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour, IProjectileSpawner
{
    public event IProjectileSpawner.ProjectileHitHandler OnHit;
    public event IProjectileSpawner.ProjectileEventHandler OnSpawn;
    public event IProjectileSpawner.ProjectileEventHandler OnDestroy;

    [SerializeField] private ProjectileData projectileData;
    private ObjectPool objectPool;

    private void Awake()
    {
        objectPool = gameObject.GetComponent<ObjectPool>();
        objectPool.InitializePool(projectileData.ProjectilePrefab);
    }

    public void SpawnProjectile()
    {
        SpawnProjectile(transform.position, transform.rotation);
    }

    public virtual void SpawnProjectile(Vector3 position, Quaternion rotation)
    {
        GameObject projectileObject = objectPool.GetObject();
        if (projectileObject == null) return;

        projectileObject.transform.SetPositionAndRotation(position, rotation);
        Projectile projectile = projectileObject.GetComponent<Projectile>();
        OnSpawn?.Invoke(projectile, this);
        projectile.InitializeProjectile(projectileData, this);
        projectile.FireProjectile(projectileObject.transform.forward);
    }

    public virtual void OnProjectileHit(Projectile projectile, RaycastHit raycastHit, bool destroyProjectile = true)
    {
        if (raycastHit.collider == null) return;

        OnHit?.Invoke(projectile, this, raycastHit);
        Debug.Log($"Hit: {raycastHit.collider.name}");
        if (raycastHit.collider.TryGetComponent(out Health health))
        {
            if (health.NetworkHealth.IsServer) health.RequestDamage(projectile.ProjectileData.Damage);
        }

        if (raycastHit.collider.TryGetComponent(out MovementController movementController))
        {
            Vector3 difference = movementController.transform.position - projectile.transform.position;
            if (movementController.NetworkMovementController.IsServer) movementController.RequestApplyForce(difference.normalized * projectile.ProjectileData.Knockback, ForceMode.Impulse);
        }

        if (destroyProjectile) projectile.DestroyProjectile();
    }

    public virtual void OnProjectileHit(Projectile projectile, IEnumerable<RaycastHit> raycastHits, bool destroyProjectile = true)
    {
        foreach (RaycastHit raycastHit in raycastHits)
        {
            OnProjectileHit(projectile, raycastHit, false);
        }

        if (destroyProjectile) projectile.DestroyProjectile();
    }

    public virtual void OnProjectileDestroy(Projectile projectile)
    {
        OnDestroy?.Invoke(projectile, this);
        projectile.ResetProjectile();
        objectPool.ReturnToPool(projectile.gameObject);
    }
}