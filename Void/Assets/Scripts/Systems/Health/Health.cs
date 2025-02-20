using System;
using Unity.Netcode;
using UnityEngine;

public class Health : MonoBehaviour
{
    public delegate void HealthHandler(float previousValue, float newValue, float maxValue);
    public event HealthHandler OnCurrentHealthChanged;
    public delegate void DeathHandler(Health health, bool isDead);
    public event DeathHandler OnDeathStateChanged;

    [SerializeField] protected Stat maxHealth = new(100);
    [SerializeField] protected Stat damageResistance = new(0);

    [SerializeField] private bool canRegenerateHealth;
    [SerializeField] protected Stat healthRegenerationRate = new(5);
    [SerializeField] protected float healthRegenerationDelay;

    private NetworkHealth networkHealth;
    private bool isDead;
    private float currentHealth;

    private float healthRegenerationDelayTimer;

    public float MaxHealth => maxHealth.Value;
    public bool IsDead => isDead;
    public float CurrentHealth => currentHealth;

    public void RequestSetCurrentHealth(float value) => networkHealth.SetCurrentHealthServerRpc(value);
    public void RequestDie() => networkHealth.SetDeathStateServerRpc(true);
    public void RequestRespawn() => networkHealth.SetDeathStateServerRpc(false);

    protected virtual void Awake()
    {
        networkHealth = GetComponent<NetworkHealth>();
    }

    private void FixedUpdate()
    {
        if (networkHealth.IsServer && !isDead)
        {
            if (canRegenerateHealth)
            {
                float deltaTime = Time.deltaTime;
                if (healthRegenerationDelayTimer <= 0)
                {
                    RequestHealing(healthRegenerationRate.Value * deltaTime);
                }
                else
                {
                    healthRegenerationDelayTimer -= deltaTime;
                }
            }
        }
    }

    public void SetCurrentHealth(float value)
    {
        if (currentHealth > value)
        {
            healthRegenerationDelayTimer = healthRegenerationDelay;
        }

        OnCurrentHealthChanged?.Invoke(currentHealth, value, MaxHealth);
        currentHealth = value;
    }

    public void SetDeathState(bool isDead)
    {
        this.isDead = isDead;
        if (networkHealth.IsServer && !isDead) RequestFullHeal();
        OnDeathStateChanged?.Invoke(this, isDead);
    }

    public void RequestServerDamage(float damage)
    {
        if (networkHealth.IsServer) RequestDamage(damage);
    }

    public void RequestDamage(float damage)
    {
        damage *= Mathf.Clamp(1 - damageResistance.Value, 0, 1);
        float newCurrentHealth = currentHealth - damage;
        RequestSetCurrentHealth(newCurrentHealth);
    }

    public void RequestHealing(float healing) => RequestSetCurrentHealth(currentHealth + healing);

    public void RequestFullHeal() => RequestSetCurrentHealth(MaxHealth);
}