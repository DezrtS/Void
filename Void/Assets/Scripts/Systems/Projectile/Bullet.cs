using UnityEngine;

public class Bullet : MonoBehaviour, IProjectile
{
    private IProjectileSpawner spawner;
    private ProjectileData projectileData;
    private Vector3 velocity;
    private bool isFired = false;
    private float lifetimeTimer = 0;

    // Temp Variables

    //private float lifetime;
    //private float gravity;
    //private float fireSpeed;
    //private float damage;
    //private LayerMask layerMask;

    //

    public IProjectileSpawner Spawner => spawner;
    public ProjectileData ProjectileData => projectileData;

    private void Update()
    {
        if (isFired)
        {
            if (Physics.Raycast(transform.position, velocity.normalized, out RaycastHit hitInfo, velocity.magnitude * Time.deltaTime, projectileData.LayerMask /*layerMask*/, QueryTriggerInteraction.Ignore))
            {
                spawner?.OnProjectileHit(this, gameObject, hitInfo.collider);
            }

            lifetimeTimer -= Time.deltaTime;
            if (lifetimeTimer <= 0)
            {
                Destroy();
            }

            velocity.y += projectileData.Gravity /*gravity*/ * Time.deltaTime;
            transform.position += velocity * Time.deltaTime;
            transform.forward = velocity.normalized;
        }
    }

    public void Initialize(IProjectileSpawner spawner, ProjectileData projectileData)
    {
        this.spawner = spawner;
        this.projectileData = projectileData;
        //lifetime = projectileData.LifetimeDuration;
        //gravity = projectileData.Gravity;
        //fireSpeed = projectileData.FireSpeed;
        //damage = projectileData.Damage;
        //layerMask = projectileData.LayerMask;
    }

    public void Fire( Vector3 direction)
    {
        if (!projectileData)
        {
            Destroy();
        }
        lifetimeTimer = projectileData.LifetimeDuration;
        velocity = projectileData.FireSpeed * direction;
        //velocity = fireSpeed * direction;
        //lifetimeTimer = lifetime;
        isFired = true;
    }


    public void Destroy()
    {
        Destroy(gameObject);
    }
}