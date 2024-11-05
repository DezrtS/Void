using UnityEngine;

public interface IProjectile
{
    IProjectileSpawner Spawner { get; }
    ProjectileData ProjectileData { get; }
    public abstract void Initialize(IProjectileSpawner spawner, ProjectileData projectileData);
    public abstract void Fire(Vector3 direction);
    public abstract void Destroy();
}