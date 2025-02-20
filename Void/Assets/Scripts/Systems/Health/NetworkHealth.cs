using Unity.Netcode;
using UnityEngine;

public class NetworkHealth : NetworkBehaviour
{
    private Health health;
    private readonly NetworkVariable<bool> isDead = new();
    private readonly NetworkVariable<float> currentHealth = new();

    private void Awake()
    {
        OnHealthInitialize();
    }

    public virtual void OnHealthInitialize()
    {
        health = GetComponent<Health>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        OnHealthSpawn();
    }

    public virtual void OnHealthSpawn()
    {
        isDead.OnValueChanged += OnDeathStateChanged;
        currentHealth.OnValueChanged += OnCurrentHealthChanged;

        if (IsServer) currentHealth.Value = health.MaxHealth;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        OnHealthDespawn();
    }

    public virtual void OnHealthDespawn()
    {
        isDead.OnValueChanged -= OnDeathStateChanged;
        currentHealth.OnValueChanged -= OnCurrentHealthChanged;
    }

    private void OnCurrentHealthChanged(float oldValue, float newValue)
    {
        if (oldValue == newValue) return;
        health.SetCurrentHealth(newValue);
    }

    private void OnDeathStateChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue) return;
        health.SetDeathState(newValue);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetCurrentHealthServerRpc(float value)
    {
        if (currentHealth.Value == value) return;

        currentHealth.Value = Mathf.Max(0, Mathf.Min(value, health.MaxHealth));
        if (currentHealth.Value <= 0)
        {
            SetDeathStateServerRpc(true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetDeathStateServerRpc(bool state)
    {
        isDead.Value = state;
    }
}