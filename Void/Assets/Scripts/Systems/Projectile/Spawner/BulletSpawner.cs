using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour, IProjectileSpawner
{
    [SerializeField] private ProjectileData projectileData;
    [SerializeField] ObjectPool objectPool;

    public event IProjectileSpawner.HitHandler OnHit;

    private void Start()
    {
        objectPool = gameObject.GetComponent<ObjectPool>();
        objectPool.InitializePool(projectileData.Prefab);
    }

    public void SpawnProjectile()
    {
        SpawnProjectile(transform.position, transform.rotation);
    }

    public void SpawnProjectile(Vector3 position, Quaternion rotation)
    {
        GameObject bullet = objectPool.GetObject();
        if (bullet == null)
        {
            return;
        }
        bullet.transform.SetPositionAndRotation(position, rotation);
        IProjectile projectile = bullet.GetComponent<IProjectile>();
        projectile.InitializeProjectile(this, projectileData);
        projectile.FireProjectile(transform.forward);
    }

    public void OnProjectileHit(IProjectile projectile, GameObject projectileGameObject, Collider hitCollider)
    {
        OnHit?.Invoke(hitCollider);
        objectPool.ReturnToPool(projectileGameObject);
    }

    public void OnProjectileDestroy(IProjectile projectile, GameObject projectileGameObject)
    {
        objectPool.ReturnToPool(projectileGameObject);
    }
}