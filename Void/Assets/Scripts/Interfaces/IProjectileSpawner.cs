using System.Collections.Generic;
using UnityEngine;

public interface IProjectileSpawner
{
    public delegate void ProjectileHitHandler(Projectile projectile, ProjectileSpawner projectileSpawner, RaycastHit raycastHit);
    public event ProjectileHitHandler OnHit;
    public delegate void ProjectileEventHandler(Projectile projectile, ProjectileSpawner projectileSpawner);
    public event ProjectileEventHandler OnDestroy;
    public event ProjectileEventHandler OnSpawn;

    public abstract void SpawnProjectile();
    public abstract void SpawnProjectile(Vector3 position, Quaternion rotation);
    public abstract void OnProjectileHit(Projectile projectile, RaycastHit raycastHit, bool destroyProjectile = true);
    public abstract void OnProjectileHit(Projectile projectile, IEnumerable<RaycastHit> raycastHits, bool destroyProjectile = true);
    public abstract void OnProjectileDestroy(Projectile projectile);
}