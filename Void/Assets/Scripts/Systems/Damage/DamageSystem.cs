using System; // Add this for the Action delegate
using UnityEngine;

public class DamageSystem : MonoBehaviour
{
    [SerializeField] private int totalHealth;
    [SerializeField] private int currentHealth;

    // Event triggered when the object takes damage
    public event Action<int, int> OnDamageTaken;

    // Event triggered when the object dies
    public event Action OnDeath;

    void Start()
    {
        currentHealth = totalHealth;

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
        currentHealth -= damage;

        OnDamageTaken?.Invoke(currentHealth, totalHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnDeath?.Invoke();
            Debug.Log("Player is Dead.");
        }
    }
}
