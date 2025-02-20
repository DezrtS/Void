public class PlayerHealth : Health
{
    protected override void Awake()
    {
        base.Awake();
        if (TryGetComponent(out PlayerStats playerStats))
        {
            maxHealth = playerStats.MaxHealth;
            healthRegenerationRate = playerStats.HealthRegenerationRate;
            damageResistance = playerStats.DamageResistance;
        }
    }
}