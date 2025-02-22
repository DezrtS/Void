using UnityEngine;

public interface IProjectile
{
    public ProjectileData ProjectileData { get; }
    public IProjectileSpawner ProjectileSpawner { get; }
    public Vector3 Velocity { get; set; }
    public Vector3 AngularVelocity { get; set; }
    public bool IsFired { get; }
    
    public abstract void InitializeProjectile(ProjectileData projectileData, IProjectileSpawner projectileSpawner);
    public abstract void FireProjectile(Vector3 direction);
    public abstract void OnProjectileHit(RaycastHit raycastHit);
    public abstract void ResetProjectile();
    public abstract void DestroyProjectile();
}