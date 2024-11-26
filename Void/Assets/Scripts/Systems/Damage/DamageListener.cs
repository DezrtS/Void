using UnityEngine;
using UnityEngine.UI;

public class DamageListener : MonoBehaviour
{
    private Image healthBar;

    public void SetHealthBar(Image healthBar)
    {
        this.healthBar = healthBar;
    }

    public void HandleDamageTaken(int currentHealth, int totalHealth)
    {
        if (!IsOwner) return; 

        if (healthBar != null)
        {
            healthBar.fillAmount = (float)currentHealth / totalHealth;
        }
    }

    public void HandleDeath()
    {
        if (!IsOwner) return;

        if (healthBar != null)
        {
            healthBar.fillAmount = 0;
        }
    }
}
