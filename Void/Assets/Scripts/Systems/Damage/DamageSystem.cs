using UnityEngine;
using Unity.Netcode;
using System;

public class DamageSystem : NetworkBehaviour
{
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    public NetworkVariable<int> totalHealth = new NetworkVariable<int>();

    public event Action<int, int> OnDamageTaken;
    public event Action OnDeath;

    private CameraController cameraController;

    private void Start()
    {
        cameraController = GetComponentInChildren<CameraController>();

        if (IsServer)
        {
            currentHealth.Value = totalHealth.Value;
        }

        currentHealth.OnValueChanged += (oldValue, newValue) =>
        {
            OnDamageTaken?.Invoke(newValue, totalHealth.Value);

            if (newValue <= 0)
            {
                TriggerDeathServerRpc();
            }
        };
    }

    public void Damage(int damage)
    {
        if (!IsServer) return;

        currentHealth.Value -= damage;

        if (currentHealth.Value <= 0)
        {
            currentHealth.Value = 0;
            TriggerDeathServerRpc();  
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TriggerDeathServerRpc()
    {
        OnDeath?.Invoke(); 
        Debug.Log("Player is Dead on the server.");
    }

    public void ResetHealth()
    {
        if (IsServer)
        {
            currentHealth.Value = totalHealth.Value;
        }
    }
}