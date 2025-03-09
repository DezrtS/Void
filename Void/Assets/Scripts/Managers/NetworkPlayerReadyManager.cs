using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerReadyManager : NetworkBehaviour
{
    private PlayerReadyManager playerReadyManager;
    private readonly NetworkVariable<bool> isAllPlayersReady = new();

    private void Awake()
    {
        playerReadyManager = GetComponent<PlayerReadyManager>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isAllPlayersReady.OnValueChanged += OnAllPlayersReadyChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        isAllPlayersReady.OnValueChanged -= OnAllPlayersReadyChanged;
    }

    private void OnAllPlayersReadyChanged(bool oldState, bool newState)
    {
        if (oldState == newState) return;
        playerReadyManager.SetAllPlayersReady(newState);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetAllPlayersReadyStateServerRpc(bool isReady)
    {
        if (isAllPlayersReady.Value == isReady) return;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SetPlayerReadyStateServerRpc(clientId, isReady);
        }
        isAllPlayersReady.Value = isReady;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerReadyStateServerRpc(ulong clientId, bool isReady)
    {
        if (playerReadyManager.PlayerReadyDictionary.ContainsKey(clientId))
        {
            SetPlayerReadyStateClientRpc(clientId, isReady);
        }
        else
        {
            AddPlayerReadyStateClientRpc(clientId, isReady);
        }

        if (isReady && !isAllPlayersReady.Value) isAllPlayersReady.Value = playerReadyManager.CheckAllPlayersReady();
        else if (!isReady && isAllPlayersReady.Value) isAllPlayersReady.Value = false;
    }

    [ClientRpc(RequireOwnership = false)]
    public void AddPlayerReadyStateClientRpc(ulong clientId, bool isReady)
    {
        playerReadyManager.AddPlayerReadyState(clientId, isReady);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerReadyStateServerRpc(ulong clientId, bool isReady)
    {
        if (!playerReadyManager.PlayerReadyDictionary.ContainsKey(clientId))
        {
            AddPlayerReadyStateClientRpc(clientId, isReady);
        }
        else
        {
            if (playerReadyManager.PlayerReadyDictionary[clientId] == isReady) return;
            SetPlayerReadyStateClientRpc(clientId, isReady);
        }

        if (isReady && !isAllPlayersReady.Value) isAllPlayersReady.Value = playerReadyManager.CheckAllPlayersReady();
        else if (!isReady && isAllPlayersReady.Value) isAllPlayersReady.Value = false;
    }

    [ClientRpc(RequireOwnership = false)]
    public void SetPlayerReadyStateClientRpc(ulong clientId, bool isReady)
    {
        playerReadyManager.SetPlayerReadyState(clientId, isReady);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemovePlayerReadyStateServerRpc(ulong clientId)
    {
        if (playerReadyManager.PlayerReadyDictionary.ContainsKey(clientId))
        {
            RemovePlayerReadyStateClientRpc(clientId);
        }
    }

    [ClientRpc(RequireOwnership = false)]
    public void RemovePlayerReadyStateClientRpc(ulong clientId)
    {
        playerReadyManager.RemovePlayerReadyState(clientId);
    }
}