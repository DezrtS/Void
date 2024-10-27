using UnityEngine;

public interface IProjectileSpawner
{
    public delegate void HitHandler(Collision collision);
    public event HitHandler OnHit;
    public abstract void SpawnProjectile();
    public abstract void OnProjectileHit(IProjectile projectile, Collision collision);
}