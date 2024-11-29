using System.Collections;
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

    public enum PlayerRole
    {
        None,
        Monster,
        Survivor,
        Spectator
    }

    private NetworkVariable<GameState> state = new NetworkVariable<GameState>(GameState.WaitingToStart);
    private Dictionary<ulong, PlayerRole> playerRoleDictionary;

    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private GameObject monsterUIPrefab;

    [SerializeField] private GameObject survivorPrefab;
    [SerializeField] private GameObject survivorUIPrefab;

    private void Awake()
    {
        playerRoleDictionary = new Dictionary<ulong, PlayerRole>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
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
        HandleGenerateGridMap();
        HandleGenerateGridMapClientRpc();
        GridMapManager.Instance.GenerateTasks();
        HandleTaskListClientRpc();
        SpawnPlayers();
    }

    private void SpawnPlayers()
    {
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (playerRoleDictionary.TryGetValue(clientId, out PlayerRole role)) {
                HandlePlayerUIClientRpc(role, clientId);
                NetworkObject networkObject;
                switch (role)
                {
                    case PlayerRole.None:
                        Debug.LogError("Player Has No Role");
                        break;
                    case PlayerRole.Monster:
                        networkObject = SpawnMonster(clientId);
                        HealthBar.Instance.AttachHealthBar(networkObject.GetComponent<IDamageable>());
                        break;
                    case PlayerRole.Survivor:
                        networkObject = SpawnSurvivor(clientId);
                        HealthBar.Instance.AttachHealthBar(networkObject.GetComponent<IDamageable>());
                        break;
                    case PlayerRole.Spectator:
                        break;
                }
            }
            else
            {
                HandlePlayerUIClientRpc(PlayerRole.Survivor, clientId);

                NetworkObject networkObject = SpawnSurvivor(clientId);
                HealthBar.Instance.AttachHealthBar(networkObject.GetComponent<IDamageable>());
            }
        }
    }

    private NetworkObject SpawnMonster(ulong clientId)
    {
        GameObject monsterGameObject = Instantiate(monsterPrefab, GridMapManager.Instance.GetMonsterSpawnPosition(), Quaternion.identity);
        NetworkObject networkObject = monsterGameObject.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId, true);
        return networkObject;
    }

    private NetworkObject SpawnSurvivor(ulong clientId)
    {
        GameObject survivorGameObject = Instantiate(survivorPrefab, GridMapManager.Instance.GetElevatorRoomPosition(), Quaternion.identity);
        NetworkObject networkObject = survivorGameObject.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId, true);
        return networkObject;
    }

    [ClientRpc]
    private void HandlePlayerUIClientRpc(PlayerRole playerRole, ulong clientId)
    {
        if (clientId == OwnerClientId)
        {
            switch (playerRole)
            {
                case PlayerRole.None:
                    Debug.LogError("Player Has No Role");
                    break;
                case PlayerRole.Monster:
                    Instantiate(monsterUIPrefab);
                    break;
                case PlayerRole.Survivor:
                    Instantiate(survivorUIPrefab);
                    break;
                case PlayerRole.Spectator:
                    break;
            }
        }
    }

    public void HandleGenerateGridMap()
    {
        GridMapManager.Instance.GenerateNewGridMap();
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
        TaskManager.Instance.DisplayTaskUI();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestPlayerRoleServerRpc(PlayerRole playerRole, ServerRpcParams serverRpcParams = default)
    {
        playerRoleDictionary[serverRpcParams.Receive.SenderClientId] = playerRole;
    }
}
