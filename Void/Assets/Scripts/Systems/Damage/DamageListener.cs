using UnityEngine;

public class DamageListener : MonoBehaviour
{
    [SerializeField] private DamageSystem damageSystem; // Reference to DamageSystem

    void OnEnable()
    {
        if (damageSystem != null)
        {
            damageSystem.OnDamageTaken += HandleDamageTaken;
            damageSystem.OnDeath += HandleDeath;
        }
    }

    void OnDisable()
    {
        if (damageSystem != null)
        {
            damageSystem.OnDamageTaken -= HandleDamageTaken;
            damageSystem.OnDeath -= HandleDeath;
        }
    }

    public void HandleDamageTaken(int currentHealth, int totalHealth)
    {
        Debug.Log($"Damage taken! Current Health: {currentHealth}/{totalHealth}");
    }

    public void HandleDeath()
    {
        Debug.Log("Object has died!");
    }
}
