using System;
using Unity.Netcode;
using UnityEngine;

public class IDamageable : NetworkBehaviour
{
    [SerializeField] private float maxHealth;
    private float health;

    public delegate void DamageHandler(float previousValue, float newValue, float maxValue);
    public event DamageHandler OnDamage;
    public event Action OnDie;

    private void Awake()
    {
        health = maxHealth;
    }

    public void Damage(float damage)
    {
        RequestApplyDamageServerRpc(damage);
    }

    public void Die()
    {
        if (!IsServer) return;

        OnDie?.Invoke();
        NetworkObject.Despawn();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestApplyDamageServerRpc(float damage, ServerRpcParams serverRpcParams = default)
    {
        HandleApplyDamage(damage);
        HandleApplyDamageClientRpc(damage);
    }

    public void HandleApplyDamage(float damage)
    {
        float newHealth = Mathf.Clamp(health - damage, 0, maxHealth);
        OnDamage?.Invoke(health, newHealth, maxHealth);
        health = newHealth;
        if (health <= 0)
        {
            health = 0;
            Die();
        }
    }

    [ClientRpc]
    public void HandleApplyDamageClientRpc(float damage)
    {
        HandleApplyDamage(damage);
    }
}
