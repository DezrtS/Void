using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.Netcode;
using UnityEditor.PackageManager;
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

    protected override void Awake()
    {
        base.Awake();
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

            PlayerReadyManager.Instance.OnAllPlayersReady += () => Loader.LoadNetwork(Loader.Scene.GameplayScene);
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

    public void StartGame()
    {
        //HandleGenerateGridMap();
        //HandleGenerateGridMapClientRpc();
        //GridMapManager.Instance.GenerateTasks();
        //HandleTaskListClientRpc();
        SpawnPlayers();
    }

    private void SpawnPlayers()
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
            
            HandlePlayerUIClientRpc(role, networkObject.NetworkObjectId, clientId);

        }
    }

    private NetworkObject SpawnMonster(ulong clientId)
    {
        GameObject monsterGameObject = Instantiate(monsterPrefab, Vector3.zero /*GridMapManager.Instance.GetMonsterSpawnPosition()*/, Quaternion.identity);
        NetworkObject networkObject = monsterGameObject.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId, true);
        return networkObject;
    }

    private NetworkObject SpawnSurvivor(ulong clientId)
    {
        GameObject survivorGameObject = Instantiate(survivorPrefab, Vector3.zero /*GridMapManager.Instance.GetElevatorRoomPosition()*/, Quaternion.identity);
        NetworkObject networkObject = survivorGameObject.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId, true);
        return networkObject;
    }

    [ClientRpc]
    private void HandlePlayerUIClientRpc(PlayerRole playerRole, ulong networkObjectId, ulong clientId)
    {
        if (clientId == OwnerClientId)
        {
            NetworkObject networkObject = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId];
            UIManager.Instance.SetupUI(playerRole, networkObject.gameObject);
        }
    }

    public void HandleGenerateGridMap()
    {
        //GridMapManager.Instance.GenerateNewGridMap();
        //TaskManager.Instance.DisplayTaskUI();
    }

    [ClientRpc]
    public void HandleGenerateGridMapClientRpc()
    {
        HandleGenerateGridMap();
    }

    [ClientRpc]
    public void HandleTaskListClientRpc()
    {
        TaskManager.Instance.RegenerateTaskInstructions();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestPlayerRoleServerRpc(PlayerRole playerRole, ServerRpcParams serverRpcParams = default)
    {
        playerRoleDictionary[serverRpcParams.Receive.SenderClientId] = playerRole;
    }
}
