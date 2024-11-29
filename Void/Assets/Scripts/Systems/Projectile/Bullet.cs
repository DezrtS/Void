using UnityEngine;

public class Bullet : MonoBehaviour, IProjectile
{
    private IProjectileSpawner spawner;
    private ProjectileData projectileData;
    private Vector3 velocity;
    private bool isFired = false;
    private float lifetimeTimer = 0;

    public IProjectileSpawner Spawner => spawner;
    public ProjectileData ProjectileData => projectileData;

    private void Update()
    {
        if (isFired)
        {
            float deltaTime = Time.deltaTime;
            if (Physics.Raycast(transform.position, velocity.normalized, out RaycastHit hitInfo, velocity.magnitude * deltaTime, projectileData.LayerMask, QueryTriggerInteraction.Ignore))
            {
                spawner?.OnProjectileHit(this, gameObject, hitInfo.collider);
            }

            lifetimeTimer -= deltaTime;
            if (lifetimeTimer <= 0)
            {
                spawner.OnProjectileDestroy(this, gameObject);
            }

            velocity.y += projectileData.Gravity * deltaTime;
            transform.position += velocity * deltaTime;
            transform.forward = velocity.normalized;
        }
    }

    public void InitializeProjectile(IProjectileSpawner spawner, ProjectileData projectileData)
    {
        this.spawner = spawner;
        this.projectileData = projectileData;
    }

    public void FireProjectile(Vector3 direction)
    {
        if (!projectileData)
        {
            spawner.OnProjectileDestroy(this, gameObject);
        }
        lifetimeTimer = projectileData.LifetimeDuration;
        velocity = projectileData.FireSpeed * direction;
        isFired = true;
    }

    public void ResetProjectile()
    {
        isFired = false;
        velocity = Vector3.zero;
        lifetimeTimer = projectileData.LifetimeDuration;
    }

    public void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}