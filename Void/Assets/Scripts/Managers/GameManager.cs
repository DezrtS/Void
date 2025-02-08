using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkSingletonPersistent<GameManager>
{
    private enum GameState
    {
        WaitingToStart,
        GamePlaying,
        GameOver
    }

    public enum PlayerRelationship
    {
        Host,
        Client,
        Server
    }

    public enum PlayerRole
    {
        None,
        Monster,
        Survivor,
        Spectator
    }

    private NetworkVariable<GameState> state = new NetworkVariable<GameState>(GameState.WaitingToStart);
    private Dictionary<ulong, PlayerRole> playerRoleDictionary;

    [SerializeField] private bool initializeNetworkingOnStart;
    [SerializeField] private PlayerRelationship defaultPlayerRelationship;

    [SerializeField] private bool startGameOnStart;
    [SerializeField] private PlayerRole defaultPlayerRole;

    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private GameObject survivorPrefab;

    [SerializeField] public GameObject FirstPersonCamera;

    private List<PlayerSpawnPoint> playerSpawnPoints = new List<PlayerSpawnPoint>();

    private void Awake()
    {
        playerRoleDictionary = new Dictionary<ulong, PlayerRole>();
    }

    private void Start()
    {
        if (initializeNetworkingOnStart)
        {
            switch (defaultPlayerRelationship)
            {
                case PlayerRelationship.Host:
                    GameMultiplayer.Instance.StartHost();
                    break;
                case PlayerRelationship.Client:
                    GameMultiplayer.Instance.StartClient();
                    break;
                default:
                    GameMultiplayer.Instance.StartHost();
                    break;
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            if (startGameOnStart)
            {
                StartGame();
                return;
            }

            PlayerReadyManager.OnAllPlayersReady += () => Loader.LoadNetwork(Loader.Scene.GameplayScene);
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneManagerLoadEventCompleted;
        }
    }

    private void OnSceneManagerLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadScene, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName == Loader.Scene.GameplayScene.ToString())
        {
            StartGame();
        }
    }

    public void AddSpawnPoint(PlayerSpawnPoint playerSpawnPoint)
    {
        if (!playerSpawnPoints.Contains(playerSpawnPoint))
        {
            playerSpawnPoints.Add(playerSpawnPoint);
        }
    }

    public PlayerSpawnPoint GetAvailablePlayerSpawnPoint(PlayerRole playerRole)
    {
        List<PlayerSpawnPoint> possibleSpawnPoints = playerSpawnPoints.FindAll(x => x.PlayerRole == playerRole && x.CanSpawn());

        if (possibleSpawnPoints.Count > 0)
        {
            int index = Random.Range(0, possibleSpawnPoints.Count);
            return possibleSpawnPoints[index];
        }

        Debug.LogWarning("No Player Spawn Point was Found");
        return null;
    }

    public void StartGame()
    {
        //HandleGenerateGridMap();
        //HandleGenerateGridMapClientRpc();
        //GridMapManager.Instance.GenerateTasks();
        //HandleTaskListClientRpc();
        SpawnPlayersServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayersServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("Spawning Players");
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Debug.Log($"Spawning Player [{clientId}]");
            PlayerRole role = defaultPlayerRole;
            playerRoleDictionary.TryGetValue(clientId, out PlayerRole playerRole);
            if (playerRole != PlayerRole.None) role = playerRole;
            NetworkObject networkObject;
            switch (role)
            {
                case PlayerRole.Monster:
                    networkObject = SpawnMonster(clientId);
                    break;
                case PlayerRole.Survivor:
                    networkObject = SpawnSurvivor(clientId);
                    break;
                default:
                    role = PlayerRole.Survivor;
                    networkObject = SpawnSurvivor(clientId);
                    break;
            }

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new List<ulong> { clientId },
                }
            };
            HandlePlayerUIClientRpc(role, networkObject.NetworkObjectId, clientId, clientRpcParams);

        }
    }

    private NetworkObject SpawnMonster(ulong clientId)
    {
        Vector3 spawnPosition = Vector3.zero;
        PlayerSpawnPoint spawnPoint = GetAvailablePlayerSpawnPoint(PlayerRole.Monster);
        if (spawnPoint != null) spawnPosition = spawnPoint.transform.position;

        GameObject monsterGameObject = Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);
        NetworkObject networkObject = monsterGameObject.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId, true);
        return networkObject;
    }

    private NetworkObject SpawnSurvivor(ulong clientId)
    {
        Vector3 spawnPosition = Vector3.zero;
        PlayerSpawnPoint spawnPoint = GetAvailablePlayerSpawnPoint(PlayerRole.Survivor);
        if (spawnPoint != null) spawnPosition = spawnPoint.transform.position;

        GameObject survivorGameObject = Instantiate(survivorPrefab, spawnPosition, Quaternion.identity);
        NetworkObject networkObject = survivorGameObject.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId, true);

        SurvivorController survivorController = survivorGameObject.GetComponent<SurvivorController>();
        survivorController.Hotbar.PickUpItem(ItemManager.SpawnItem(GameDataManager.Instance.GetItemData(0)));
        return networkObject;
    }

    [ClientRpc(RequireOwnership = false)]
    private void HandlePlayerUIClientRpc(PlayerRole playerRole, ulong networkObjectId, ulong clientId, ClientRpcParams rpcParams = default)
    {
        ItemManager.CreateSimpleEventLog("HandlePlayerUIEvent", $"PlayerRole: {playerRole}, NetworkObjectId: {networkObjectId}, ClientId: {clientId}");

        NetworkObject networkObject = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId];
        UIManager.Instance.SetupUI(playerRole, networkObject.gameObject);
    }

    public void HandleGenerateGridMap()
    {
        //GridMapManager.Instance.GenerateNewGridMap();
        //TaskManager.Instance.DisplayTaskUI();
    }

    [ClientRpc(RequireOwnership = false)]
    public void HandleGenerateGridMapClientRpc()
    {
        HandleGenerateGridMap();
    }

    [ClientRpc(RequireOwnership = false)]
    public void HandleTaskListClientRpc()
    {
        TaskManager.Instance.RegenerateTaskInstructions();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestPlayerRoleServerRpc(PlayerRole playerRole, ServerRpcParams serverRpcParams = default)
    {
        playerRoleDictionary[serverRpcParams.Receive.SenderClientId] = playerRole;
        ItemManager.CreateSimpleEventLog("Player Join", $"{serverRpcParams.Receive.SenderClientId} : {playerRole}");
    }
}
