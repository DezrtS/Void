using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameMultiplayer : NetworkSingleton<GameMultiplayer>
{
    public enum PlayerRelationship { Client, Host, Server }

    [Header("Options")]
    [SerializeField] private bool loadGameplaySceneOnAllPlayersReady;
    [SerializeField] private bool initializeNetworkingOnStart;
    [SerializeField] private PlayerRelationship defaultPlayerRelationship;

    private void Start()
    {
        if (initializeNetworkingOnStart)
        {
            StartPlayer(defaultPlayerRelationship);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            PlayerReadyManager.OnAllPlayersReady += LoadGameplayScene;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneManagerLoadEventCompleted;

            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedCallback;
        }
    }

    public void LoadGameplayScene()
    {
        if (!loadGameplaySceneOnAllPlayersReady) return;
        PlayerReadyManager.OnAllPlayersReady -= LoadGameplayScene;
        PlayerReadyManager.Instance.RequestSetAllPlayersReadyState(false);
        Loader.LoadNetwork(Loader.Scene.GameplayScene);
    }

    private void OnSceneManagerLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadScene, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName == Loader.Scene.GameplayScene.ToString())
        {
            GameManager.Instance.RequestSetGameState(GameManager.GameState.WaitingToStart);
        }
    }

    private void OnClientDisconnectedCallback(ulong clientId)
    {
        CleanUpPlayer(clientId);
        NotifyPlayerLeftClientRpc(clientId);
    }

    public void CleanUpPlayer(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var networkClient))
        {
            NetworkObject playerObject = networkClient.PlayerObject;
            if (playerObject != null)
            {
                if (GameManager.Instance.PlayerRoleDictionary.ContainsKey(clientId))
                {
                    switch (GameManager.Instance.PlayerRoleDictionary[clientId])
                    {
                        case GameManager.PlayerRole.Survivor:
                            playerObject.GetComponent<Hotbar>().RequestDropEverything();
                            playerObject.GetComponent<Draggable>().RequestStopUsing();
                            break;
                        case GameManager.PlayerRole.Monster:
                            break;
                        default:
                            break;
                    }
                }

                playerObject.Despawn();
            }
        }

        GameManager.Instance.RequestRemovePlayerRole(clientId);
        PlayerReadyManager.Instance.RequestRemovePlayerReadyState(clientId);
    }

    [ClientRpc]
    private void NotifyPlayerLeftClientRpc(ulong clientId)
    {
        Debug.Log($"Player with ID {clientId} has left the game.");
        // Update UI or other client-side logic
    }

    public static void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public static void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    public static void StartPlayer(PlayerRelationship playerRelationship)
    {
        switch (playerRelationship)
        {
            case PlayerRelationship.Host:
                StartHost();
                break;
            case PlayerRelationship.Client:
                StartClient();
                break;
            default:
                StartHost();
                break;
        }
    }

    public static ClientRpcParams GenerateClientRpcParams(ServerRpcParams rpcParams, bool reverse = false)
    {
        ulong callingClientId = rpcParams.Receive.SenderClientId;
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = (reverse) ? NetworkManager.Singleton.ConnectedClientsIds.Where(id => id == callingClientId).ToArray() : NetworkManager.Singleton.ConnectedClientsIds.Where(id => id != callingClientId).ToArray()
            }
        };
        return clientRpcParams;
    }
}
