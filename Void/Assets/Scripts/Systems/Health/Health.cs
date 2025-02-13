using System;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SerializeField] protected Stat maxHealth = new Stat(100);
    protected Stat health = new Stat(0);

    [SerializeField] private bool canRegenerateHealth;
    [SerializeField] protected Stat healthRegenerationRate;
    [SerializeField] protected float healthRegenerationDelay;

    [SerializeField] protected Stat damageResistance;

    public delegate void HealthHandler(float previousValue, float newValue, float maxValue);
    public event HealthHandler OnHealthChanged;
    public event Action<Health> OnDeath;

    private float healthRegenerationDelayTimer;
    private bool healthChanged = false;

    protected virtual void Awake()
    {
        health.SetBaseValue(maxHealth.Value);
    }

    private void FixedUpdate()
    {
        if (canRegenerateHealth && healthChanged)
        {
            float deltaTime = Time.deltaTime;
            if (healthRegenerationDelayTimer <= 0)
            {
                float maxHealth = GetMaxHealth();
                float previousHealth = GetHealth();
                float newHealth = previousHealth + healthRegenerationRate.Value * deltaTime;

                if (newHealth >= maxHealth)
                {
                    healthChanged = false;
                    OnHealthChanged?.Invoke(previousHealth, maxHealth, GetMaxHealth());
                }
                else
                {
                    OnHealthChanged?.Invoke(previousHealth, newHealth, GetMaxHealth());
                }

                SetHealth(newHealth);
            }
            else
            {
                healthRegenerationDelayTimer -= deltaTime;
            }
        }
    }

    public float GetMaxHealth()
    {
        return maxHealth.Value;
    }

    public float GetHealth()
    {
        return Mathf.Clamp(health.Value, 0, maxHealth.Value);
    }

    public void SetHealth(float value)
    {
        health.SetBaseValue(Mathf.Min(value, maxHealth.Value));
    }

    public void Damage(float amount)
    {
        amount = amount * Mathf.Clamp(1 - damageResistance.Value, 0, 1);
        if (amount <= 0) return;

        UpdateHealthClientServerSide(-amount);
        UpdateHealthServerRpc(-amount);
    }

    public void Heal(float amount)
    {
        UpdateHealthClientServerSide(amount);
        UpdateHealthServerRpc(amount);
    }

    public void Die()
    {
        OnDeath?.Invoke(this);
    }

    public void UpdateHealthClientServerSide(float amount)
    {
        if (amount < 0)
        {
            healthChanged = true;
            healthRegenerationDelayTimer = healthRegenerationDelay;
        }

        float previousHealth = GetHealth();
        float newHealth = previousHealth + amount;
        OnHealthChanged?.Invoke(previousHealth, newHealth, GetMaxHealth());
        SetHealth(newHealth);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateHealthServerRpc(float amount, ServerRpcParams serverRpcParams = default)
    {
        UpdateHealthClientServerSide(amount);
        ClientRpcParams clientRpcParams = GameMultiplayer.GenerateClientRpcParams(serverRpcParams);
        UpdateHealthClientRpc(amount, clientRpcParams);

        if (GetHealth() <= 0)
        {
            Die();
            DieClientRpc();
        }
    }

    [ClientRpc(RequireOwnership = false)]
    public void UpdateHealthClientRpc(float amount, ClientRpcParams clientRpcParams = default)
    {
        UpdateHealthClientServerSide(amount);
    }

    [ClientRpc(RequireOwnership = false)]
    public void DieClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Die();
    }
}
