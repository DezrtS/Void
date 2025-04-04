using UnityEngine;

public class AOEProjectile : Projectile
{
    private AOEProjectileData aOEProjectileData;

    public override void InitializeProjectile(ProjectileData projectileData, ProjectileSpawner projectileSpawner)
    {
        base.InitializeProjectile(projectileData, projectileSpawner);
        aOEProjectileData = projectileData as AOEProjectileData;
    }

    public override void OnProjectileHit(RaycastHit raycastHit)
    {
        if (!GameManager.CanDamage(raycastHit.collider.gameObject, projectileRole)) return;
        if (aOEProjectileData.HitEffect != null) Instantiate(aOEProjectileData.HitEffect, transform.position, Quaternion.identity);
        RaycastHit[] raycastHits = new RaycastHit[30];
        Physics.SphereCastNonAlloc(transform.position, aOEProjectileData.Range, Vector3.forward, raycastHits, aOEProjectileData.Range, aOEProjectileData.LayerMask);
        
        ProjectileSpawner.OnProjectileHit(this, raycastHits);
    }
}