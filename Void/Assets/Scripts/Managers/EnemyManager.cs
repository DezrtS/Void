using Unity.Netcode;
using UnityEngine;

public class EnemyManager : NetworkSingleton<EnemyManager>
{
    [SerializeField] private bool spawnEnemies = true;
    [SerializeField] private GameObject voidBeastPrefab;
    [SerializeField] private float spawnNewEnemyAfter;

    private NetworkedObjectPool voidBeastObjectPool;
    private float spawnEnemyTimer = 0;
    private bool gameStarted;

    protected override void OnEnable()
    {
        base.OnEnable();
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameManager.GameState gameState)
    {
        if (IsServer && gameState == GameManager.GameState.ReadyToStart)
        {
            voidBeastObjectPool = GetComponent<NetworkedObjectPool>();
            voidBeastObjectPool.InitializePool(voidBeastPrefab);
            spawnEnemyTimer = spawnNewEnemyAfter;
        }
    }

    private void FixedUpdate()
    {
        if (!IsServer || !gameStarted || !spawnEnemies) return;

        if (spawnEnemyTimer <= 0)
        {
            SpawnNewEnemy();
            spawnEnemyTimer = spawnNewEnemyAfter;
        }
        else
        {
            spawnEnemyTimer -= Time.deltaTime;
        }
    }

    public void SpawnNewEnemy()
    {
        GameObject voidBeast = voidBeastObjectPool.GetObject();
        if (voidBeast == null) return;

        VoidBeastController voidBeastController = voidBeast.GetComponent<VoidBeastController>();
        EnableDisableEnemyClientRpc(voidBeastController.NetworkObjectId, true);
        voidBeastController.Health.OnDeathStateChanged += OnEnemyDeath;

        Spawnpoint spawnpoint = SpawnManager.Instance.GetRandomSpawnpoint(Spawnpoint.SpawnpointType.VoidBeast);
        if (spawnpoint == null) return;

        voidBeastController.NavMeshMovement.Teleport(spawnpoint.Spawn());
        voidBeastController.Activate();
    }

    public void SpawnEnemy(Vector3 position)
    {
        GameObject voidBeast = voidBeastObjectPool.GetObject();
        if (voidBeast == null) return;

        VoidBeastController voidBeastController = voidBeast.GetComponent<VoidBeastController>();
        voidBeastController.Health.OnDeathStateChanged += OnEnemyDeath;

        voidBeastController.NavMeshMovement.Teleport(position);
        voidBeastController.Activate();

        EnableDisableEnemyClientRpc(voidBeastController.NetworkObjectId, true);
    }

    public void OnEnemyDeath(Health health, bool isDead)
    {
        if (isDead)
        {
            health.OnDeathStateChanged -= OnEnemyDeath;
            health.RequestRespawn();
            voidBeastObjectPool.ReturnToPool(health.gameObject);
            EnableDisableEnemyClientRpc(health.gameObject.GetComponent<NetworkObject>().NetworkObjectId, false);
        }
    }

    [ClientRpc(RequireOwnership = false)]
    public void EnableDisableEnemyClientRpc(ulong networkObjectId, bool enable)
    {
        NetworkObject networkObject = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId];
        networkObject.gameObject.SetActive(enable);
    }
}