using UnityEngine;

public class VoidBeastPodMutation : ProjectileMutation
{
    [SerializeField] private int voidBeastSpawnCount;

    private void Start()
    {
        projectileSpawner.OnDestroy += OnProjectileDestroy;
    }

    private void OnProjectileDestroy(Projectile projectile, ProjectileSpawner projectileSpawner)
    {
        if (!projectile.HasExpired())
        {
            for (int i = 0; i < voidBeastSpawnCount; i++)
            {
                EnemyManager.Instance.SpawnEnemy(projectile.transform.position);
            }
        }
    }
}
