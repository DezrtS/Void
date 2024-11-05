using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour, IProjectileSpawner
{
    [SerializeField] private ProjectileData projectileData;

    public event IProjectileSpawner.HitHandler OnHit;

    public void SpawnProjectile()
    {
        GameObject bullet = Instantiate(projectileData.Prefab, transform.position, Quaternion.identity);
        IProjectile projectile = bullet.GetComponent<IProjectile>();
        projectile.Initialize(this, projectileData);
        projectile.Fire(transform.forward);
    }

    public void OnProjectileHit(IProjectile projectile, GameObject projectileGameObject, Collider hitCollider)
    {
        OnHit?.Invoke(hitCollider);
        projectile.Destroy();
    }
}