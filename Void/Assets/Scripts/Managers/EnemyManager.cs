using Unity.Netcode;
using UnityEngine;

public class EnemyManager : NetworkSingleton<EnemyManager>
{
    [SerializeField] private GameObject voidBeastPrefab;
    [SerializeField] private float spawnNewEnemyAfter;

    private NetworkedObjectPool voidBeastObjectPool;
    private float spawnEnemyTimer = 0;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            voidBeastObjectPool = GetComponent<NetworkedObjectPool>();
            voidBeastObjectPool.InitializePool(voidBeastPrefab);
            spawnEnemyTimer = spawnNewEnemyAfter;
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
        EnableDisableEnemyClientRpc(voidBeastController.NetworkObjectId, true);
        voidBeastController.Health.OnDeath += OnEnemyDeath;

        Spawnpoint spawnpoint = SpawnManager.Instance.GetRandomSpawnpoint(Spawnpoint.SpawnpointType.VoidBeast);
        if (spawnpoint == null) return;

        voidBeastController.NavMeshMovement.Teleport(spawnpoint.Spawn());
        voidBeastController.Activate();
    }

    public void OnEnemyDeath(Health health)
    {
        health.OnDeath -= OnEnemyDeath;
        health.SetHealth(health.GetMaxHealth());
        voidBeastObjectPool.ReturnToPool(health.gameObject);
        EnableDisableEnemyClientRpc(health.gameObject.GetComponent<NetworkObject>().NetworkObjectId, false);
    }

    [ClientRpc(RequireOwnership = false)]
    public void EnableDisableEnemyClientRpc(ulong networkObjectId, bool enable)
    {
        NetworkObject networkObject = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId];
        networkObject.gameObject.SetActive(enable);
    }
}
