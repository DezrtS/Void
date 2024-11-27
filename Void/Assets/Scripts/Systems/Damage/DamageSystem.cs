using System; 
using UnityEngine;
using Unity.Netcode;

public class DamageSystem : NetworkBehaviour
{
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
        };
    }

    public void Damage(int damage)
    {
        if (!IsServer) return; 

        currentHealth.Value -= damage; 

        OnDamageTaken?.Invoke(currentHealth.Value, totalHealth.Value);

        if (currentHealth.Value <= 0)
        {
            currentHealth.Value = 0;
            OnDeath?.Invoke();
            Debug.Log("Player is Dead.");
        }
    }

    private void HandleHealthChanged(int previousValue, int newValue)
    {
        OnDamageTaken?.Invoke(newValue, totalHealth.Value);

        if (newValue <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    public void ResetHealth()
    {
        if (IsServer)
        {
            currentHealth.Value = totalHealth.Value;
        }
    }
}
