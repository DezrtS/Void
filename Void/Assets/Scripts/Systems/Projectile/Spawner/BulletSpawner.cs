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
        objectPool.InitializePool(projectileData.Prefab, 5, true);
        
    }
    public void SpawnProjectile()
    {
        //GameObject bullet = Instantiate(projectileData.Prefab, transform.position, Quaternion.identity);
        GameObject bullet = objectPool.GetObject();
        bullet.transform.position = transform.position;

        IProjectile projectile = bullet.GetComponent<IProjectile>();
        projectile.Initialize(this, projectileData);
        projectile.Fire(transform.forward);
    }

    public void OnProjectileHit(IProjectile projectile, GameObject projectileGameObject, Collider hitCollider)
    {
        objectPool.ReturnToPool(projectileGameObject);
        OnHit?.Invoke(hitCollider);
        //projectile.Destroy();
    }
}