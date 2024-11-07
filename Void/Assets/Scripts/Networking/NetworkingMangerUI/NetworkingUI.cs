using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button serverBtn;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button joinBtn;
    [SerializeField] private Button monsterBtn;
    [SerializeField] private GameObject monsterPre;
    [SerializeField] private GameObject playerPre;

    // Dictionary to track player types
    private Dictionary<ulong, bool> playerTypes = new Dictionary<ulong, bool>();

    private void Awake() 
    {
        serverBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartServer();
        });
        
        hostBtn.onClick.AddListener(() => {
            // Host will always be a regular player
            RequestSpawnServerRpc(NetworkManager.Singleton.LocalClientId, false);
            NetworkManager.Singleton.StartHost();
        });
        
        joinBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
            StartCoroutine(WaitForConnection(false));  // Join as regular player
        });
        
        monsterBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
            StartCoroutine(WaitForConnection(true));  // Join as monster
        });
    }

    private IEnumerator WaitForConnection(bool isMonster)
    {
        // Wait until the client is connected and has a valid LocalClientId
        yield return new WaitUntil(() => NetworkManager.Singleton.IsClient && NetworkManager.Singleton.LocalClientId != 0);
        
        // Request our spawn type once the client is connected
        RequestSpawnServerRpc(NetworkManager.Singleton.LocalClientId, isMonster);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnServerRpc(ulong clientId, bool asMonster)
    {
        // Store the player type
        playerTypes[clientId] = asMonster;

        // Now that we have stored the player's type, the server can spawn the player
        SpawnPlayer(clientId);
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        // Ensure this is only called by the server
        if (!NetworkManager.Singleton.IsServer) return;

        // Only spawn if we know their type
        if (playerTypes.ContainsKey(clientId))
        {
            SpawnPlayer(clientId);
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        // Ensure this is only called by the server
        if (!NetworkManager.Singleton.IsServer) return;

        // Check if this client already has a player object
        foreach (NetworkObject netObj in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
        {
            if (netObj.OwnerClientId == clientId)
            {
                Debug.Log($"Client {clientId} already has a spawned player object!");
                return;
            }
        }

        // Determine which prefab to spawn based on the player's type
        bool isMonster = playerTypes.ContainsKey(clientId) && playerTypes[clientId];
        GameObject prefabToSpawn = isMonster ? monsterPre : playerPre;
        
        Debug.Log($"Spawning {(isMonster ? "monster" : "player")} for client {clientId}");
        
        GameObject player = Instantiate(prefabToSpawn);
        NetworkObject networkObject = player.GetComponent<NetworkObject>();

        if (networkObject != null)
        {
            networkObject.SpawnAsPlayerObject(clientId);
        }
        else
        {
            Debug.LogError($"Prefab does not have a NetworkObject component.");
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}
