using System;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SerializeField] private float maxHealth;
    private float health;

    public delegate void HealthHandler(float previousValue, float newValue, float maxValue);
    public event HealthHandler OnHealthChanged;
    public event Action OnDeath;

    private void Awake()
    {
        health = maxHealth;
    }

    public void Damage(float amount)
    {
        RequestHealthChangeServerRpc(-amount);
    }

    public void Heal(float amount)
    {
        RequestHealthChangeServerRpc(amount);
    }

    public void Die()
    {
        OnDeath?.Invoke();
        NetworkObject.Despawn();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestHealthChangeServerRpc(float amount, ServerRpcParams serverRpcParams = default)
    {
        HandleHealthChange(amount);
        HandleHealthChangeClientRpc(health);

        if (health == 0)
        {
            Die();
        }
    }

    public void HandleHealthChange(float amount)
    {
        float newHealth = Mathf.Clamp(health + amount, 0, maxHealth);
        OnHealthChanged?.Invoke(health, newHealth, maxHealth);
        health = Mathf.Clamp(newHealth, 0, maxHealth);
    }

    [ClientRpc]
    public void HandleHealthChangeClientRpc(float newHealth)
    {
        OnHealthChanged?.Invoke(health, newHealth, maxHealth);
        health = Mathf.Clamp(newHealth, 0, maxHealth);
    }
}
