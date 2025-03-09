using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerReadyManager : Singleton<PlayerReadyManager>
{
    public delegate void PlayerReadyHandler(ulong clientId);
    public static event PlayerReadyHandler OnPlayerReady;
    public static event Action OnAllPlayersReady;

    private NetworkPlayerReadyManager networkPlayerReadyManager;
    private bool isAllPlayersReady;

    private Dictionary<ulong, bool> playerReadyDictionary = new Dictionary<ulong, bool>();

    public NetworkPlayerReadyManager NetworkPlayerReadyManager => networkPlayerReadyManager;
    public bool IsAllPlayersReady => isAllPlayersReady;
    public Dictionary<ulong, bool> PlayerReadyDictionary => playerReadyDictionary;

    public void RequestSetAllPlayersReadyState(bool isReady) => networkPlayerReadyManager.SetAllPlayersReadyStateServerRpc(isReady);
    public void RequestAddPlayerReadyState(ulong clientId, bool isReady) => networkPlayerReadyManager.AddPlayerReadyStateServerRpc(clientId, isReady);
    public void RequestSetPlayerReadyState(ulong clientId, bool isReady) => networkPlayerReadyManager.SetPlayerReadyStateServerRpc(clientId, isReady);
    public void RequestRemovePlayerReadyState(ulong clientId) => networkPlayerReadyManager.RemovePlayerReadyStateServerRpc(clientId);

    private void Awake()
    {
        networkPlayerReadyManager = GetComponent<NetworkPlayerReadyManager>();
    }

    public bool CheckAllPlayersReady()
    {
        bool allPlayersReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                allPlayersReady = false;
                break;
            }
        }
        return allPlayersReady;
    }

    public void SetAllPlayersReady(bool isAllPlayersReady)
    {
        this.isAllPlayersReady = isAllPlayersReady;
        Debug.Log($"ALL PLAYER READY STATE UPDATE: {isAllPlayersReady}");
        if (isAllPlayersReady) OnAllPlayersReady?.Invoke();
    }

    public void AddPlayerReadyState(ulong clientId, bool isReady)
    {
        if (playerReadyDictionary.ContainsKey(clientId))
        {
            SetPlayerReadyState(clientId, isReady);
        }
        else
        {
            playerReadyDictionary.Add(clientId, isReady);
            if (isReady) OnPlayerReady?.Invoke(clientId);
        }
    }

    public void SetPlayerReadyState(ulong clientId, bool isReady)
    {
        if (playerReadyDictionary.ContainsKey(clientId))
        {
            playerReadyDictionary[clientId] = isReady;
            if (isReady) OnPlayerReady?.Invoke(clientId);
        }
        else
        {
            AddPlayerReadyState(clientId, isReady);
        }
    }

    public void RemovePlayerReadyState(ulong clientId)
    {
        if (playerReadyDictionary.ContainsKey(clientId))
        {
            playerReadyDictionary.Remove(clientId);
        }
    }
}