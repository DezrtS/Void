using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class DamageListener : NetworkBehaviour
{
    [SerializeField] private Image healthBar;
    [SerializeField] private DamageSystem damageSystem;
    private CameraController cameraController;

    private void Start()
    {
        damageSystem = GetComponent<DamageSystem>();
        
        cameraController = GetComponentInChildren<CameraController>();

        if (damageSystem != null)
        {
            damageSystem.OnDamageTaken += HandleDamageTaken;
            damageSystem.OnDeath += HandleDeath;
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

        if (cameraController != null)
        {
            cameraController.DetachCamera();
        }
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
            damageSystem.OnDeath -= HandleDeath;
        }
    }
}
