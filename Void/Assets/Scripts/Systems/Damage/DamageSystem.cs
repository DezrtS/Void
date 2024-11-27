using UnityEngine;
using Unity.Netcode;
using System;

public class DamageSystem : NetworkBehaviour
{
    public NetworkVariable<bool> isDead = new NetworkVariable<bool>(
        false, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    public NetworkVariable<int> totalHealth = new NetworkVariable<int>();

    public event Action<int, int> OnDamageTaken;
    public event Action OnDeath;

    private void Start()
    {
        if (IsServer)
        {
            currentHealth.Value = totalHealth.Value;
        }

        currentHealth.OnValueChanged += (oldValue, newValue) =>
        {
            OnDamageTaken?.Invoke(newValue, totalHealth.Value);

            if (newValue <= 0 && !isDead.Value)
            {
                isDead.Value = true;
                OnDeath?.Invoke();  
                TriggerDeathClientRpc();
            }
        };
    }

    public void Damage(int damage)
    {
        if (!IsServer) return;

        currentHealth.Value -= damage;

        if (currentHealth.Value <= 0 && !isDead.Value)
        {
            currentHealth.Value = 0;
            isDead.Value = true;
            OnDeath?.Invoke();  
            TriggerDeathClientRpc();
        }
    }

    [ServerRpc]
    private void TriggerDeathServerRpc()
    {
        if (!isDead.Value)
        {
            isDead.Value = true;
            OnDeath?.Invoke();
            Debug.Log("Player is Dead on the server.");
        }
    }

    [ClientRpc]
    private void TriggerDeathClientRpc()
    {
        Debug.Log("Player has died on the client.");
        OnDeath?.Invoke();
    }

    public void ResetHealth()
    {
        if (IsServer)
        {
            currentHealth.Value = totalHealth.Value;
            isDead.Value = false;
        }
    }
}