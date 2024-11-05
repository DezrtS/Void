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
            if (Physics.Raycast(transform.position, velocity.normalized, out RaycastHit hitInfo, velocity.magnitude * Time.deltaTime, projectileData.LayerMask, QueryTriggerInteraction.Ignore))
            {
                spawner?.OnProjectileHit(this, gameObject, hitInfo.collider);
            }

            lifetimeTimer -= Time.deltaTime;
            if (lifetimeTimer <= 0)
            {
                Destroy();
            }

            velocity.y += projectileData.Gravity * Time.deltaTime;
            transform.position += velocity * Time.deltaTime;
            transform.forward = velocity.normalized;
        }
    }

    public void Initialize(IProjectileSpawner spawner, ProjectileData projectileData)
    {
        this.spawner = spawner;
        this.projectileData = projectileData;
    }

    public void Fire( Vector3 direction)
    {
        if (!projectileData)
        {
            Destroy();
        }
        lifetimeTimer = projectileData.LifetimeDuration;
        velocity = projectileData.FireSpeed * direction;
        isFired = true;
    }


    public void Destroy()
    {
        Destroy(gameObject);
    }
}