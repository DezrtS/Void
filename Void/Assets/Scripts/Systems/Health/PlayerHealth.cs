public class PlayerHealth : Health
{
    protected override void Awake()
    {
        if (TryGetComponent(out PlayerStats playerStats))
        {
            maxHealth = playerStats.MaxHealth;
            healthRegenerationRate = playerStats.HealthRegenerationRate;
            damageResistance = playerStats.DamageResistance;
        }

        base.Awake();
    }
}