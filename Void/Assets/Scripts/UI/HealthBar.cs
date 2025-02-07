using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Image heathbarImage;
    private Health health;

    private void OnEnable()
    {
        UIManager.OnSetupUI += OnSetupUI;

        if (health)
        {
            health.OnHealthChanged += OnHealthBarChange;
            health.OnDeath += DetachHealthBar;
        }
    }

    private void OnDisable()
    {
        UIManager.OnSetupUI -= OnSetupUI;

        if (health)
        {
            health.OnHealthChanged -= OnHealthBarChange;
            health.OnDeath -= DetachHealthBar;
        }
    }

    private void Awake()
    {
        heathbarImage = GetComponent<Image>();
    }

    public void OnSetupUI(GameObject player)
    {
        if (player.TryGetComponent(out Health health))
        {
            AttachHealthBar(health);
        }
    }

    public void AttachHealthBar(Health health)
    {
        this.health = health;
        health.OnHealthChanged += OnHealthBarChange;
        health.OnDeath += DetachHealthBar;
    }

    public void DetachHealthBar(Health health)
    {
        health.OnHealthChanged -= OnHealthBarChange;
        health.OnDeath -= DetachHealthBar;
        health = null;
    }

    public void OnHealthBarChange(float previousValue, float newValue, float maxValue)
    {
        float percentage = newValue / maxValue;
        heathbarImage.fillAmount = percentage;
    }
}