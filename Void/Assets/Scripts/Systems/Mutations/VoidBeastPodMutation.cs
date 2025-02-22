using UnityEngine;

public class VoidBeastPodMutation : ProjectileMutation
{
    private void Start()
    {
        projectileSpawner.OnDestroy += OnProjectileDestroy;
    }

    private void OnProjectileDestroy(Projectile projectile, IProjectileSpawner projectileSpawner)
    {
        if (!projectile.HasExpired()) EnemyManager.Instance.SpawnEnemy(projectile.transform.position);
    }
}
