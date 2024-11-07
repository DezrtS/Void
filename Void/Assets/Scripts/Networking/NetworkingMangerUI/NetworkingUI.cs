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
    [SerializeField] private Button spawnBtn;  // New button for spawning
    [SerializeField] private GameObject specialPlayerPrefab; // Different prefab for spawn button

    private void Awake() 
    {
        serverBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartServer();
        });
        hostBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
        });
        joinBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
        });

        spawnBtn.onClick.AddListener(SpawnSpecialPlayer);
        spawnBtn.gameObject.SetActive(false); // Hide until client or host connects

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy() 
    {
        // Clean up event to avoid memory leaks
        if (NetworkManager.Singleton != null) {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Show the spawn button when a client connects
        if (clientId == NetworkManager.Singleton.LocalClientId) {
            spawnBtn.gameObject.SetActive(true);
        }
    }

    private void SpawnSpecialPlayer()
    {
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost) 
        {
            if (specialPlayerPrefab != null)
            {
                // Spawn the special prefab instead of the default player prefab
                var specialPlayerInstance = Instantiate(specialPlayerPrefab);
                specialPlayerInstance.GetComponent<NetworkObject>().Spawn();
            }
            else
            {
                Debug.LogError("Special Player Prefab is not assigned in NetworkManagerUI.");
            }
        }
    }
}

