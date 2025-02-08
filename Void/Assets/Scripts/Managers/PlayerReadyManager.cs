using System;
using System.Collections.Generic;
using Unity.Netcode;

public class PlayerReadyManager : NetworkSingletonPersistent<PlayerReadyManager>
{
    private Dictionary<ulong, bool> playerReadyDictionary;

    public delegate void PlayerReadyHandler(ulong clientId);
    public static event PlayerReadyHandler OnPlayerReady;
    public static event Action OnAllPlayersReady;

    private void Awake()
    {
        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;
        OnPlayerReady?.Invoke(serverRpcParams.Receive.SenderClientId);

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
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
