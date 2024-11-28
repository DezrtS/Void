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
            ActivateGeneralHUDServerRpc();
            ActivateHUD("Host");
        });

        joinBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            StartCoroutine(ReplaceHostTempWithPlayer());
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

    private IEnumerator ReplaceHostTempWithPlayer()
    {
        yield return new WaitForSeconds(0.1f);

        if (NetworkManager.Singleton.IsHost)
        {
            var localPlayerObject = NetworkManager.Singleton.LocalClient?.PlayerObject;
            if (localPlayerObject != null)
            {
                localPlayerObject.Despawn(true);
            }

            SpawnPlayerServerRPC(NetworkManager.Singleton.LocalClientId, GetSpawnPosition());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRPC(ulong clientId, Vector3 spawnPosition)
    {
        var playerInstance = Instantiate(playerPre, spawnPosition, Quaternion.identity);
        var networkObject = playerInstance.GetComponent<NetworkObject>();

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
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected");
    }

    private Vector3 GetSpawnPosition()
    {
        return new Vector3(0f, 2f, 0f);
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
            ActivateGeneralHUDClientRpc();
        }

        [ClientRpc]
        private void ActivateGeneralHUDClientRpc()
        {
            if (generalHUD != null)
            {
                generalHUD.SetActive(true);
            }

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
