using System; // For Action delegate
using UnityEngine;
using Unity.Netcode;

public class DamageSystem : NetworkBehaviour
{
    [SerializeField] private int totalHealth;
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>();

    public event Action<int, int> OnDamageTaken;

    public event Action OnDeath;

    private void Start()
    {
        if (IsServer)
        {
            currentHealth.Value = totalHealth;
        }

        currentHealth.OnValueChanged += HandleHealthChanged;

        DamageListener damageListener = FindObjectOfType<DamageListener>();
        if (damageListener != null)
        {
            OnDamageTaken += damageListener.HandleDamageTaken;
            OnDeath += damageListener.HandleDeath;
        }
        else
        {
            Debug.LogWarning("No DamageListener found in the scene!");
        }
    }

    public void Damage(int damage)
    {
        if (!IsServer)
        {
            Debug.LogWarning("Damage should only be applied on the server!");
            return;
        }

        currentHealth.Value = Mathf.Max(currentHealth.Value - damage, 0);

        if (currentHealth.Value > 0)
        {
            OnDamageTaken?.Invoke(currentHealth.Value, totalHealth);
        }
        else
        {
            OnDeath?.Invoke();
            Debug.Log("Player is Dead.");
        }
    }

    private void HandleHealthChanged(int previousValue, int newValue)
    {
        OnDamageTaken?.Invoke(newValue, totalHealth);

        if (newValue <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    public void ResetHealth()
    {
        if (IsServer)
        {
            currentHealth.Value = totalHealth;
        }
    }
}
