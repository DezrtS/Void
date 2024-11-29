using UnityEngine;

public interface IProjectile
{
    IProjectileSpawner Spawner { get; }
    ProjectileData ProjectileData { get; }
    public abstract void InitializeProjectile(IProjectileSpawner spawner, ProjectileData projectileData);
    public abstract void FireProjectile(Vector3 direction);
    public abstract void ResetProjectile();
    public abstract void DestroyProjectile();
}