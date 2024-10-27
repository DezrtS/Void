using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour, IProjectileSpawner
{
    [SerializeField] private ProjectileData projectileData;
    //private List<IProjectile> spawnedProjectiles;

    public event IProjectileSpawner.HitHandler OnHit;

    //private void Awake()
    //{
    //    spawnedProjectiles = new List<IProjectile>();
    //}

    public void SpawnProjectile()
    {
        GameObject bullet = Instantiate(projectileData.Prefab, transform.position, Quaternion.identity);
        IProjectile projectile = bullet.GetComponent<IProjectile>();
        projectile.Initialize(this, projectileData);
        projectile.Fire(transform.forward);
        //spawnedProjectiles.Add(projectile);
    }

    public void OnProjectileHit(IProjectile projectile, Collision collision)
    {
        OnHit?.Invoke(collision);
        projectile.Destroy();
    }
}