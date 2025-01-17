using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerReadyManager : NetworkSingletonPersistent<PlayerReadyManager>
{
    private Dictionary<ulong, bool> playerReadyDictionary;

    public delegate void PlayerReadyHandler(ulong clientId);
    public event PlayerReadyHandler OnPlayerReady;
    public event Action OnAllPlayersReady;

    private void Awake()
    {
        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        playerReadyDictionary[clientId] = true;
        OnPlayerReady?.Invoke(clientId);

        bool allClientsReady = true;
        foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(id) || !playerReadyDictionary[id])
            {
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            OnAllPlayersReady?.Invoke();
        }
    }
}