using UnityEngine;

public class Bullet : MonoBehaviour, IProjectile
{
    private Rigidbody rig;
    private IProjectileSpawner spawner;
    private ProjectileData projectileData;
    private bool isFired = false;
    private float lifetimeTimer = 0;

    public IProjectileSpawner Spawner => spawner;
    public ProjectileData ProjectileData => projectileData;

    private void Awake()
    {
        rig = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (isFired)
        {
            lifetimeTimer -= Time.fixedDeltaTime;
            if (lifetimeTimer <= 0 )
            {
                Destroy();
            }
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
        rig.AddForce(projectileData.FireSpeed * direction, ForceMode.Impulse);
        isFired = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        spawner?.OnProjectileHit(this, collision);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}