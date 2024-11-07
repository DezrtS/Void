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

    private bool isMonster = false;
    
    private void Awake() 
    {
        serverBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartServer();
        });
        hostBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
        });
        joinBtn.onClick.AddListener(() => {
            isMonster = false;
        });
        monsterBtn.onClick.AddListener(() => {
            isMonster = true;
        });
    }

    private void Start()
    {
        // Ensure the code runs only on the server
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if(isMonster){
            // Spawn the player object for the connected client
            GameObject player = Instantiate(monsterPre);  // Assign your player prefab here
            NetworkObject networkObject = player.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                networkObject.SpawnAsPlayerObject(clientId);
            }
            else
            {
                Debug.LogError("Player prefab does not have a NetworkObject component.");
            }
        }
        else{
            // Spawn the player object for the connected client
            GameObject player = Instantiate(playerPre);  // Assign your player prefab here
            NetworkObject networkObject = player.GetComponent<NetworkObject>();

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

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}