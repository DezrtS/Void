using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public class HealthChange
    {
        private float totalHealthChange; // Total health change to apply
        private float duration;         // Duration over which to apply the change
        private float elapsedTime;      // Time elapsed since the health change started
        private float healthChangePerSecond; // Health change per second

        public HealthChange(float totalHealthChange, float duration)
        {
            this.totalHealthChange = totalHealthChange;
            this.duration = duration;
            elapsedTime = 0f;
            healthChangePerSecond = totalHealthChange / duration; // Precompute health change per second
        }

        /// <summary>
        /// Applies the health change over time.
        /// </summary>
        /// <param name="health">The Health component to modify.</param>
        /// <param name="deltaTime">Time since the last frame.</param>
        /// <returns>True if the health change is complete, false otherwise.</returns>
        public bool Apply(Health health, float deltaTime)
        {
            // Calculate the remaining duration
            float remainingDuration = duration - elapsedTime;

            // If the duration has expired, return true to indicate completion
            if (remainingDuration <= 0f)
            {
                return true;
            }

            // Calculate the health change for this frame
            float healthChangeThisFrame = healthChangePerSecond * deltaTime;

            // Apply the health change
            health.RequestUpdateCurrentHealth(healthChangeThisFrame);

            // Update elapsed time
            elapsedTime += deltaTime;

            // Return false if the health change is still in progress
            return false;
        }
    }


    public delegate void HealthHandler(float previousValue, float newValue, float maxValue);
    public event HealthHandler OnCurrentHealthChanged;
    public delegate void DeathHandler(Health health, bool isDead);
    public event DeathHandler OnDeathStateChanged;

    [SerializeField] protected Stat maxHealth = new(100);
    [SerializeField] protected Stat damageResistance = new(0);

    [SerializeField] private bool canRegenerateHealth;
    [SerializeField] protected Stat healthRegenerationRate = new(5);
    [SerializeField] protected float healthRegenerationDelay;

    [SerializeField] private EventReference hurtSound;
    [SerializeField] private EventReference deathSound;

    private NetworkHealth networkHealth;
    private bool isDead;
    private float currentHealth;

    private float healthRegenerationDelayTimer;

    private List<HealthChange> healthChanges = new List<HealthChange>();

    public float MaxHealth => maxHealth.Value;
    public NetworkHealth NetworkHealth => networkHealth;
    public bool IsDead => isDead;
    public float CurrentHealth => currentHealth;

    public void RequestUpdateCurrentHealth(float change) => networkHealth.UpdateCurrentHealthServerRpc(change);
    public void RequestSetCurrentHealth(float value) => networkHealth.SetCurrentHealthServerRpc(value);
    public void RequestDie() => networkHealth.SetDeathStateServerRpc(true);
    public void RequestRespawn() => networkHealth.SetDeathStateServerRpc(false);
    public void RequestHealthChangeOverTime(float healthChange, float duration) => networkHealth.ChangeHealthOverTimeServerRpc(healthChange, duration); 

    protected virtual void Awake()
    {
        networkHealth = GetComponent<NetworkHealth>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (networkHealth.IsOwner && isDead) RequestRespawn();
        }
    }

    private void FixedUpdate()
    {
        if (networkHealth.IsServer && !isDead)
        {
            float deltaTime = Time.fixedDeltaTime;

            if (canRegenerateHealth)
            {
                if (healthRegenerationDelayTimer <= 0)
                {
                    RequestHealing(healthRegenerationRate.Value * deltaTime);
                }
                else
                {
                    healthRegenerationDelayTimer -= deltaTime;
                }
            }

            for (int i = healthChanges.Count - 1; i >= 0; i--)
            {
                if (healthChanges[i].Apply(this, deltaTime))
                {
                    healthChanges.RemoveAt(i);
                }
            }
        }


    }

    public void SetCurrentHealth(float value)
    {
        if (currentHealth > value)
        {
            healthRegenerationDelayTimer = healthRegenerationDelay;
            AudioManager.PlayOneShot(hurtSound, gameObject);
        }

        OnCurrentHealthChanged?.Invoke(currentHealth, value, MaxHealth);
        currentHealth = value;
    }

    public void SetDeathState(bool isDead)
    {
        this.isDead = isDead;
        if (isDead) AudioManager.PlayOneShot(deathSound, gameObject);
        if (networkHealth.IsServer && !isDead) RequestFullHeal();
        OnDeathStateChanged?.Invoke(this, isDead);
    }

    public void RequestDamage(float damage)
    {
        damage *= Mathf.Clamp(1 - damageResistance.Value, 0, 1);
        float newCurrentHealth = currentHealth - damage;
        RequestSetCurrentHealth(newCurrentHealth);
    }

    public void RequestHealing(float healing) => RequestSetCurrentHealth(currentHealth + healing);

    public void RequestFullHeal() => RequestSetCurrentHealth(MaxHealth);

    public void ChangeHealthOverTime(float healthChange, float duration)
    {
        healthChanges.Add(new HealthChange(healthChange, duration));
    }
}