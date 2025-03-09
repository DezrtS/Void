using Unity.Netcode;
using UnityEngine;
using static GameManager;

public class NetworkGameManager : NetworkBehaviour
{
    [Header("Options")]
    [SerializeField] private bool startGameOnSpawn;

    private GameManager gameManager;
    private readonly NetworkVariable<GameState> state = new();

    private void Awake()
    {
        gameManager = GetComponent<GameManager>();   
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        state.OnValueChanged += OnGameStateChanged;
        if (IsServer && startGameOnSpawn) SetGameStateServerRpc(GameState.WaitingToStart);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        state.OnValueChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState oldState, GameState newState)
    {
        if (oldState == newState) return;
        gameManager.SetGameState(newState);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetGameStateServerRpc(GameState gameState)
    {
        if (state.Value == gameState) return;
        state.Value = gameState;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerRoleServerRpc(ulong clientId, PlayerRole playerRole)
    {
        if (gameManager.PlayerRoleDictionary.ContainsKey(clientId))
        {
            SetPlayerRoleClientRpc(clientId, playerRole);
        }
        else
        {
            AddPlayerRoleClientRpc(clientId, playerRole);
        }
    }

    [ClientRpc(RequireOwnership = false)]
    public void AddPlayerRoleClientRpc(ulong clientId, PlayerRole playerRole)
    {
        gameManager.PlayerRoleDictionary.Add(clientId, playerRole);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerRoleServerRpc(ulong clientId, PlayerRole newPlayerRole)
    {
        if (!gameManager.PlayerRoleDictionary.ContainsKey(clientId))
        {
            AddPlayerRoleClientRpc(clientId, newPlayerRole);
        }
        else
        {
            if (gameManager.PlayerRoleDictionary[clientId] == newPlayerRole) return;
            SetPlayerRoleClientRpc(clientId, newPlayerRole);
        }
    }

    [ClientRpc(RequireOwnership = false)]
    public void SetPlayerRoleClientRpc(ulong clientId, PlayerRole newPlayerRole)
    {
        gameManager.PlayerRoleDictionary[clientId] = newPlayerRole;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemovePlayerRoleServerRpc(ulong clientId)
    {
        if (!gameManager.PlayerRoleDictionary.ContainsKey(clientId)) return;
        RemovePlayerRoleClientRpc(clientId);
    }

    [ClientRpc(RequireOwnership = false)]
    public void RemovePlayerRoleClientRpc(ulong clientId)
    {
        gameManager.PlayerRoleDictionary.Remove(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayersServerRpc()
    {
        gameManager.SpawnPlayers();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerServerRpc(ulong clientId)
    {
        gameManager.SpawnPlayer(clientId);
    }
}