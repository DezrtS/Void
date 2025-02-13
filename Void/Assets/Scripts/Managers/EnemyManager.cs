using UnityEngine;

public class EnemyManager : NetworkSingleton<EnemyManager>
{
    [SerializeField] private GameObject voidBeastPrefab;
    [SerializeField] private float spawnNewEnemyAfter;

    private ObjectPool voidBeastObjectPool;
    private float spawnEnemyTimer = 0;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            voidBeastObjectPool = GetComponent<ObjectPool>();
            voidBeastObjectPool.InitializePool(voidBeastPrefab);
        }
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        if (spawnEnemyTimer > 0)
        {
            spawnEnemyTimer -= Time.deltaTime;
            if (spawnEnemyTimer <= 0)
            {
                SpawnNewEnemy();
                spawnEnemyTimer = spawnNewEnemyAfter;
            }
        }
    }

    public void SpawnNewEnemy()
    {
        GameObject voidBeast = voidBeastObjectPool.GetObject();
        if (voidBeast == null) return;

        VoidBeastController voidBeastController = voidBeast.GetComponent<VoidBeastController>();
        voidBeastController.Health.OnDeath += OnEnemyDeath;

        Spawnpoint spawnpoint = SpawnManager.Instance.GetRandomSpawnpoint(Spawnpoint.SpawnpointType.VoidBeast);
        if (spawnpoint == null) return;

        voidBeastController.NavMeshMovement.Teleport(spawnpoint.Spawn());
    }

    public void OnEnemyDeath(Health health)
    {
        health.SetHealth(health.GetMaxHealth());
        voidBeastObjectPool.ReturnToPool(health.gameObject);
    }
}
