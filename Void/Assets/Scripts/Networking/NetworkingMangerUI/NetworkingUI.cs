using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;

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
            StartCoroutine(ReplaceHostTempWithPlayer());
            ActivateHUD("Client");
        });
    }

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        ActivateGeneralHUDServerRpc();
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
    }

    [ServerRpc(RequireOwnership = false)]
    private void ActivateGeneralHUDServerRpc()
    {
        ActivateGeneralHUDClientRpc(true);
        Debug.Log("General HUD is active on Server");
    }

    [ClientRpc]
    private void ActivateGeneralHUDClientRpc(bool state)
    {
        if (generalHUD != null)
        {
            generalHUD.SetActive(state);
        }

        if (state)
        {
            ActivateTimer();
        }
        Debug.Log("General HUD is active on Client");
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

            SpawnMonsterServerRpc(NetworkManager.Singleton.LocalClientId, GridMapManager.Instance.GetMonsterSpawnPosition());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnMonsterServerRpc(ulong clientId, Vector3 spawnPosition)
    {
        var monsterInstance = Instantiate(monsterPre, spawnPosition, Quaternion.identity);
        var networkObject = monsterInstance.GetComponent<NetworkObject>();

        Debug.Log(spawnPosition + " Monster Location");

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

            Debug.Log("Player About to be moved");
            SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId, GridMapManager.Instance.GetElevatorRoomPosition());
            Debug.Log("Player has been moved");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ulong clientId, Vector3 playerSpawn)
    {
        var playerInstance = Instantiate(playerPre, playerSpawn, Quaternion.identity);
        var networkObject = playerInstance.GetComponent<NetworkObject>();

        Debug.Log($"[Server] Player Location " + playerSpawn);

        if (networkObject != null)
        {
            networkObject.SpawnWithOwnership(clientId);
            NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject = networkObject;
        }
        else
        {
            Debug.LogError("Player prefab does not have a NetworkObject component.");
        }
    }
}
