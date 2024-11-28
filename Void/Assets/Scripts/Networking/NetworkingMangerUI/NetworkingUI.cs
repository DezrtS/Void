using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button serverBtn;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button joinBtn;
    [SerializeField] private GameObject monsterPre;
    [SerializeField] private GameObject playerPre;
    [SerializeField] private GameObject monsterHUD;
    [SerializeField] private GameObject playerHUD;
    [SerializeField] private GameObject generalHUD;
    [SerializeField] private GameObject networkManagerUI;
    [SerializeField] private GameObject timerObject;

    private void Awake()
    {
        if (timerObject != null)
        {
            timerObject.SetActive(false);
        }

        serverBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
            ActivateHUD("Server");
        });

        hostBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            StartCoroutine(ReplaceHostPlayerWithMonster());
            ActivateGeneralHUDServerRpc();  // Activate general HUD for both the host and clients
            ActivateHUD("Host");
        });

        joinBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            StartCoroutine(MovePlayerToSpawnPoint(clientId));
            ActivateHUD("Client");
        });
    }

    private IEnumerator ReplaceHostPlayerWithMonster()
    {
        yield return new WaitForSeconds(0.1f);

        if (NetworkManager.Singleton.IsHost)
        {
            var localPlayerObject = NetworkManager.Singleton.LocalClient?.PlayerObject;
            if (localPlayerObject != null)
            {
                localPlayerObject.Despawn(true);
            }

            SpawnMonsterServerRpc(NetworkManager.Singleton.LocalClientId, GetSpawnPosition());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnMonsterServerRpc(ulong clientId, Vector3 spawnPosition)
    {
        var monsterInstance = Instantiate(monsterPre, spawnPosition, Quaternion.identity);
        var networkObject = monsterInstance.GetComponent<NetworkObject>();

        if (networkObject != null)
        {
            networkObject.SpawnWithOwnership(clientId);
            NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject = networkObject;
        }
        else
        {
            Debug.LogError("Monster prefab does not have a NetworkObject component.");
        }
    }

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected.");
        
        if (NetworkManager.Singleton.IsServer)
        {
            // Set a spawn position for the client using ServerRpc
            Vector3 spawnPosition = GetRandomSpawnPosition();
            SetPlayerSpawnPositionServerRpc(clientId, spawnPosition);
        }
    }

    [ServerRpc]
    private void SetPlayerSpawnPositionServerRpc(ulong clientId, Vector3 spawnPosition)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient client))
        {
            if (client.PlayerObject != null)
            {
                SetPlayerPositionClientRpc(clientId, spawnPosition);
            }
        }
    }


    [ClientRpc]
    private void SetPlayerPositionClientRpc(ulong clientId, Vector3 spawnPosition)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            var playerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
            if (playerObject != null)
            {
                playerObject.transform.position = spawnPosition;
            }
        }
    }


    private IEnumerator MovePlayerToSpawnPoint(ulong clientId)
    {
        yield return new WaitForSeconds(0.1f);

        // Ensure this is only executed on the server
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkClient client;
            while (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out client) || client.PlayerObject == null)
            {
                yield return null;
            }

            // Get the player's network object
            var playerObject = client.PlayerObject;
            if (playerObject != null)
            {
                Vector3 spawnPosition = GetRandomSpawnPosition();
                playerObject.transform.position = spawnPosition;

                Debug.Log($"Player {clientId} moved to spawn point: {spawnPosition}");
            }
            else
            {
                Debug.LogError($"Player object for client {clientId} could not be found.");
            }
        }
        else
        {
            Debug.LogWarning("MovePlayerToSpawnPoint is called on the client, but it should only be called on the server.");
        }
    }


    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected");
    }

    private Vector3 GetSpawnPosition()
    {
        return new Vector3(0f, 3f, 0f);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float x = Random.Range(-2.5f, 2.5f);
        float z = Random.Range(-2.5f, 2.5f);
        float y = 1f;
        return new Vector3(x, y, z);
    }

    private void ActivateHUD(string role)
    {
        if (networkManagerUI != null)
        {
            networkManagerUI.SetActive(false);
        }

        if (role == "Server" || role == "Host")
        {
            if (monsterHUD != null)
            {
                monsterHUD.SetActive(true);
            }
        }
        else if (role == "Client")
        {
            if (playerHUD != null)
            {
                playerHUD.SetActive(true);
            }
        }

        Debug.Log("General HUD active: " + (generalHUD.activeSelf ? "Yes" : "No"));
    }

        [ServerRpc]
        private void ActivateGeneralHUDServerRpc()
        {
            // Call the ClientRpc to activate the HUD on all clients (and the server)
            ActivateGeneralHUDClientRpc();
        }

        // ClientRpc to activate the generalHUD for all clients
        [ClientRpc]
        private void ActivateGeneralHUDClientRpc()
        {
            if (generalHUD != null)
            {
                generalHUD.SetActive(true);
            }

            // Optionally, activate the timer on the clients
            ActivateTimer();
        }

    private void ActivateTimer()
    {
        if (timerObject != null)
        {
            timerObject.SetActive(true);
            var timeManager = timerObject.GetComponent<TimeManager>();
            timeManager?.StartTimer();
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
}
