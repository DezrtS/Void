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
    [SerializeField] private GameObject monsterPre;
    [SerializeField] private GameObject playerPre;

    private void Awake() 
    {
        // Start a server only (no player object for server)
        serverBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartServer();
        });

        // Host starts server and spawns as monster
        hostBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
            StartCoroutine(ReplaceHostPlayerWithMonster());
        });

        // Clients join as regular players
        joinBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
        });
    }

    private IEnumerator ReplaceHostPlayerWithMonster()
    {
        // Wait briefly to ensure the default player object is spawned
        yield return new WaitForSeconds(0.1f);

        if (NetworkManager.Singleton.IsHost)
        {
            // Check if the host player object is spawned and then despawn it
            if (NetworkManager.Singleton.LocalClient.PlayerObject != null)
            {
                NetworkObject playerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
                playerObject.Despawn(true);  // Despawn the player object
            }

            // Instantiate and spawn the monster prefab for the host
            GameObject monsterInstance = Instantiate(monsterPre, GetRandomSpawnPosition(), Quaternion.identity);
            NetworkObject networkObject = monsterInstance.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                // Spawn the monster and assign ownership to the host
                networkObject.SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);
                NetworkManager.Singleton.LocalClient.PlayerObject = networkObject;
            }
        }
    }

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Spawn a regular player object for clients joining the game
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
    }

    private Vector3 GetRandomSpawnPosition()
    {
        // Define a spawn area for players
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
            }
        }
    }
}
