using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class DamageListener : NetworkBehaviour
{
    [SerializeField]private Image healthBar;  
    [SerializeField]private DamageSystem damageSystem;  

    private void Start()
    {
        damageSystem = GetComponent<DamageSystem>();

        if (damageSystem != null)
        {
            damageSystem.OnDamageTaken += HandleDamageTaken;
        }
        else
        {
            Debug.LogWarning("DamageSystem component not found!");
        }
    }

    public void HandleDamageTaken(int currentHealth, int totalHealth)
    {
        UpdateHealthBar(currentHealth, totalHealth);
    }

    public void HandleDeath()
    {
        Debug.Log("Player has died.");
    }

    public void SetHealthBar(Image healthBarPrefab)
    {
        healthBar = healthBarPrefab;
        if (damageSystem != null)
        {
            UpdateHealthBar(damageSystem.currentHealth.Value, damageSystem.totalHealth.Value);
        }
    }

    private void UpdateHealthBar(int currentHealth, int totalHealth)
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)currentHealth / totalHealth; 
        }
    }

    private void OnDestroy()
    {
        if (damageSystem != null)
        {
            damageSystem.OnDamageTaken -= HandleDamageTaken;
        }
    }
}
