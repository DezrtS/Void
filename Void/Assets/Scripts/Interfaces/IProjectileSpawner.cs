using UnityEngine;

public interface IProjectileSpawner
{
    public delegate void HitHandler(Collider hitCollider);
    public event HitHandler OnHit;
    public abstract void SpawnProjectile();
    public abstract void SpawnProjectile(Vector3 position, Quaternion rotation);
    public abstract void OnProjectileHit(IProjectile projectile, GameObject projectileGameObject, Collider hitCollider);
}