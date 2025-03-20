using Unity.VisualScripting;
using UnityEngine;

public class GrapplingProjectile : Projectile
{
    private LineRenderer lineRenderer;

    public override void InitializeProjectile(ProjectileData projectileData, ProjectileSpawner projectileSpawner)
    {
        base.InitializeProjectile(projectileData, projectileSpawner);
        lineRenderer = GetComponent<LineRenderer>();
    }

    public override void UpdateProjectile(float deltaTime)
    {
        base.UpdateProjectile(deltaTime);
        if (IsFired) lineRenderer.SetPositions(new Vector3[] { transform.position, ProjectileSpawner.transform.position });
    }
}