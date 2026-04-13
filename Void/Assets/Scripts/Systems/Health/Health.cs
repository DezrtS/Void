using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

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
    
    private struct DamageEntry
    {
        public float damage;
        public float time;

        public DamageEntry(float damage, float time)
        {
            this.damage = damage;
            this.time = time;
        }
    }

    [SerializeField] private float damageTrackingWindow = 5f;

    private readonly Queue<DamageEntry> damageHistory = new Queue<DamageEntry>();
    private float totalDamageInWindow;


    public delegate void HealthHandler(float previousValue, float newValue, float maxValue);
    public event HealthHandler OnCurrentHealthChanged;
    public delegate void DeathHandler(Health health, bool isDead);
    public event DeathHandler OnDeathStateChanged;

    [SerializeField] protected Stat maxHealth = new(100);
    [SerializeField] protected Stat damageResistance = new(0);

    [SerializeField] private bool canRegenerateHealth;
    [SerializeField] protected Stat healthRegenerationRate = new(5);
    [SerializeField] protected float healthRegenerationDelay;

    [SerializeField] private VisualEffect playOnHurt;

    [SerializeField] private bool playHeartbeat;
    [SerializeField] private float gruntThreshold;

    [SerializeField] private EventReference heartbeatSound;
    [SerializeField] private EventReference hurtSound;
    [SerializeField] private EventReference gruntSound;
    [SerializeField] private EventReference reflectSound;
    [SerializeField] private EventReference deathSound;

    private NetworkHealth networkHealth;
    private bool isDead;
    private float currentHealth;

    private EventInstance heartbeatInstance;
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

    private void Start()
    {
        if (!networkHealth.IsOwner) return;
        if (playHeartbeat)
        {
            heartbeatInstance = AudioManager.CreateEventInstance(heartbeatSound, gameObject);
            heartbeatInstance.start();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (!networkHealth.IsOwner) return;
            if (isDead) RequestRespawn();
            else RequestDie();
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

        if (networkHealth.IsOwner && playHeartbeat)
        {
            heartbeatInstance.setParameterByName("Heartbeat", 1 - (currentHealth / maxHealth.Value));
        }
    }

    public void SetCurrentHealth(float value)
    {
        if (currentHealth > value)
        {
            healthRegenerationDelayTimer = healthRegenerationDelay;
            AudioManager.PlayOneShot(hurtSound, gameObject);
            if (playOnHurt != null) playOnHurt.Play();
        }

        OnCurrentHealthChanged?.Invoke(currentHealth, value, MaxHealth);
        currentHealth = value;
    }

    public void SetDeathState(bool isDead)
    {
        this.isDead = isDead;
        if (isDead) AudioManager.PlayOneShot(deathSound, gameObject);
        OnDeathStateChanged?.Invoke(this, isDead);
    }

    public void RequestDamage(float damage)
    {
        damage *= Mathf.Clamp(1 - damageResistance.Value, 0, 1);

        if (networkHealth.IsServer && damage > 0f)
        {
            RecordDamage(damage);
        }

        float newCurrentHealth = currentHealth - damage;

        if (damage >= gruntThreshold) AudioManager.RequestPlayOneShot(gruntSound, transform.position);
        if (damage <= 0) AudioManager.RequestPlayOneShot(reflectSound, transform.position);

        RequestSetCurrentHealth(newCurrentHealth);
    }
    
    private void RecordDamage(float damage)
    {
        float currentTime = Time.time;

        damageHistory.Enqueue(new DamageEntry(damage, currentTime));
        totalDamageInWindow += damage;

        CleanupOldDamage(currentTime);
    }

    private void CleanupOldDamage(float currentTime)
    {
        while (damageHistory.Count > 0 &&
               currentTime - damageHistory.Peek().time > damageTrackingWindow)
        {
            var old = damageHistory.Dequeue();
            totalDamageInWindow -= old.damage;
        }
    }
    public void RequestHealing(float healing) => RequestSetCurrentHealth(currentHealth + healing);

    public void RequestFullHeal() => RequestSetCurrentHealth(MaxHealth);

    public void ChangeHealthOverTime(float healthChange, float duration)
    {
        healthChanges.Add(new HealthChange(healthChange, duration));
    }
    
    private float GetRecentDPS()
    {
        if (!networkHealth.IsServer) return 0f;

        CleanupOldDamage(Time.time);

        if (damageHistory.Count == 0) return 0f;

        float duration = Time.time - damageHistory.Peek().time;
        if (duration <= 0f) return 0f;

        return totalDamageInWindow / duration;
    }
    
    public bool WillDieInTime(float time)
    {
        if (!networkHealth.IsServer) return false;

        float dps = GetRecentDPS();
        if (dps <= 0f) return false;

        float expectedDamage = dps * time;

        return expectedDamage >= currentHealth;
    }
}