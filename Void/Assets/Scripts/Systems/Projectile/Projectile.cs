using UnityEngine;

public class Projectile : MonoBehaviour, IProjectile
{
    private ProjectileData projectileData;
    private ProjectileSpawner projectileSpawner;

    private Vector3 velocity;
    private Vector3 angularVelocity;
    protected bool isFired;

    private Vector3 spinAngles;
    private float lifetimeTimer;

    private bool hasHit;
    private RaycastHit lastHit;

    public ProjectileData ProjectileData => projectileData;
    public ProjectileSpawner ProjectileSpawner => projectileSpawner;
    public Vector3 Velocity { get { return velocity; } set { velocity = value; } }
    public Vector3 AngularVelocity { get { return angularVelocity; } set { angularVelocity = value; } }
    public bool IsFired => isFired;

    public bool HasExpired() => lifetimeTimer <= 0;

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        UpdateTimers(deltaTime);
        UpdateProjectile(deltaTime);
    }

    public virtual void InitializeProjectile(ProjectileData projectileData, ProjectileSpawner projectileSpawner)
    {
        this.projectileData = projectileData;
        this.projectileSpawner = projectileSpawner;
    }

    public void FireProjectile(Vector3 direction)
    {
        if (!projectileData)
        {
            projectileSpawner.OnProjectileDestroy(this);
            return;
        }

        if (TryGetComponent(out TrailRenderer trailRenderer))
        {
            trailRenderer.Clear();
        }

        velocity = projectileData.FireSpeed * direction;
        angularVelocity = projectileData.AngularVelocity;
        lifetimeTimer = projectileData.LifetimeDuration;
        isFired = true;
    }

    public virtual void OnProjectileHit(RaycastHit raycastHit)
    {
        if (projectileData.HitEffect != null) Instantiate(projectileData.HitEffect, transform.position, Quaternion.LookRotation(-raycastHit.normal), raycastHit.transform);
        AudioManager.PlayOneShot(projectileData.HitSound, gameObject);
        projectileSpawner.OnProjectileHit(this, raycastHit);
    }

    public virtual void ResetProjectile()
    {
        isFired = false;
        velocity = Vector3.zero;
        angularVelocity = Vector3.zero;
        spinAngles = Vector3.zero;
        lifetimeTimer = projectileData.LifetimeDuration;
        hasHit = false;
    }

    public virtual void DestroyProjectile()
    {
        projectileSpawner.OnProjectileDestroy(this);
    }

    public virtual void UpdateTimers(float deltaTime)
    {
        if (lifetimeTimer > 0)
        {
            lifetimeTimer -= deltaTime;
        }
    }

    public virtual void UpdateProjectile(float deltaTime)
    {
        if (!isFired) return;

        if (hasHit)
        {
            OnProjectileHit(lastHit);
            hasHit = false;
            return;
        }

        if (projectileData.UseBoxCast)
        {
            float travelDistance = velocity.magnitude * deltaTime;
            Vector3 halfExtents = new Vector3(projectileData.BoxCastSize.x * 0.5f, projectileData.BoxCastSize.y * 0.5f, travelDistance);
            Quaternion orientation = Quaternion.LookRotation(velocity);

            if (Physics.BoxCast(transform.position, halfExtents, velocity.normalized, out RaycastHit hitInfo, orientation, travelDistance, projectileData.LayerMask, QueryTriggerInteraction.Ignore))
            {
                DebugDraw.DrawBoxCast(transform.position, halfExtents, velocity.normalized, orientation, travelDistance, Color.red, 5);
                //DebugDraw.DrawBoxCast(transform.position + velocity * deltaTime, halfExtents, velocity.normalized, orientation, projectileData.BoxCastSize.y, Color.green, 5);
                //OnProjectileHit(hitInfo);
                lastHit = hitInfo;
                hasHit = true;

                transform.position = hitInfo.point;
                UpdateRotation(deltaTime);
            }
            else
            {
                UpdatePosition(deltaTime);
                UpdateRotation(deltaTime);
            }
        }
        else
        {
            if (Physics.Raycast(transform.position, velocity.normalized, out RaycastHit hitInfo, velocity.magnitude, projectileData.LayerMask, QueryTriggerInteraction.Ignore))
            {
                lastHit = hitInfo;
                hasHit = true;

                transform.position = hitInfo.point;
                UpdateRotation(deltaTime);
            }
            else
            {
                UpdatePosition(deltaTime);
                UpdateRotation(deltaTime);
            }
        }

        if (HasExpired()) DestroyProjectile();
    }

    public void UpdatePosition(float deltaTime)
    {
        velocity.y += projectileData.Gravity * deltaTime;
        transform.position += velocity * deltaTime;
    }

    public void UpdateRotation(float deltaTime)
    {
        if (velocity != Vector3.zero)
        {
            spinAngles += angularVelocity * (360 * deltaTime);
            Quaternion baseRotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);
            Quaternion spinRotation = Quaternion.Euler(spinAngles);
            transform.rotation = baseRotation * spinRotation;
        }
    }
}