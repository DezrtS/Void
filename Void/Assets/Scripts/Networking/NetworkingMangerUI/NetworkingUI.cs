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
    [SerializeField] private Image healthBarPrefab;  
    [SerializeField] private Transform uiCanvas;     

    private void Awake()
    {
        // Start server only
        serverBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartServer();
        });

        // Host starts server and spawns monster
        hostBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
            StartCoroutine(ReplaceHostPlayerWithMonster());
        });

        // Clients join the game
        joinBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
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

            var monsterInstance = Instantiate(monsterPre, GetRandomSpawnPosition(), Quaternion.identity);
            var networkObject = monsterInstance.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                networkObject.SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);
                NetworkManager.Singleton.LocalClient.PlayerObject = networkObject;
            }
            else
            {
                Debug.LogError("Monster prefab does not have a NetworkObject component.");
            }
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
        if (!NetworkManager.Singleton.IsHost || clientId != NetworkManager.Singleton.LocalClientId)
        {
            GameObject playerInstance = Instantiate(playerPre, GetRandomSpawnPosition(), Quaternion.identity);
            NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                networkObject.SpawnAsPlayerObject(clientId);
            }
            else
            {
                Debug.LogError("Player prefab does not have a NetworkObject component.");
            }
        }

        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            AssignHealthBarToLocalPlayer(clientId);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected");
    }

    private void AssignHealthBarToLocalPlayer(ulong clientId)
    {
        NetworkObject localPlayer = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        if (localPlayer != null)
        {
            DamageListener damageListener = localPlayer.GetComponent<DamageListener>();
            if (damageListener != null)
            {
                Image healthBar = Instantiate(healthBarPrefab, uiCanvas);
                damageListener.SetHealthBar(healthBar); 
            }
            else
            {
                Debug.LogWarning("DamageListener not found on local player!");
            }
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float randomX = Random.Range(-5f, 5f);
        float randomZ = Random.Range(-5f, 5f);
        return new Vector3(randomX, 3f, randomZ);
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }
    }
}
