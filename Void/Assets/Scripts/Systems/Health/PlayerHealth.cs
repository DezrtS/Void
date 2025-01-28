using UnityEngine;

public class PlayerHealth : Health
{
    [SerializeField] private Stat maxHealth = new Stat(100);
    private Stat health = new Stat(0);

    public Stat MaxHealth => maxHealth;
    public Stat Health => health;

    protected override void Awake()
    {
        base.Awake();

        if (TryGetComponent(out PlayerStats playerStats)) maxHealth = playerStats.MaxHealth;
        health.SetBaseValue(maxHealth.Value);
    }

    public override float GetMaxHealth()
    {
        return maxHealth.Value;
    }

    public override float GetHealth()
    {
        return Mathf.Clamp(health.Value, 0, maxHealth.Value);
    }

    public override void SetHealth(float value)
    {
        health.SetBaseValue(Mathf.Min(value, maxHealth.Value));
    }
}
