using UnityEngine;
using UnityEngine.UI;

public class HealthBar : Singleton<HealthBar>
{
    private Image heathbarImage;
    private IDamageable damageable;

    private void Awake()
    {
        heathbarImage = GetComponent<Image>();
    }

    public void AttachHealthBar(IDamageable damageable)
    {
        this.damageable = damageable;
        damageable.OnDamage += OnHealthBarChange;
        damageable.OnDie += DetachHealthBar;
    }

    public void DetachHealthBar()
    {
        damageable.OnDamage -= OnHealthBarChange;
        damageable.OnDie -= DetachHealthBar;
        damageable = null;
    }

    public void OnHealthBarChange(float previousValue, float newValue, float maxValue)
    {
        float percentage = newValue / maxValue;
        heathbarImage.fillAmount = percentage;
    }
}